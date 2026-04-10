const state = {
  gatewayUrl: localStorage.getItem('smartsureGatewayUrl') || 'http://localhost:5000',
  token: localStorage.getItem('smartsureToken') || '',
  roles: JSON.parse(localStorage.getItem('smartsureRoles') || '[]'),
  user: JSON.parse(localStorage.getItem('smartsureUser') || 'null')
};

const $ = (selector) => document.querySelector(selector);

const elements = {
  gatewayUrl: $('#gatewayUrl'),
  connectionBadge: $('#connectionBadge'),
  tokenBadge: $('#tokenBadge'),
  roleBadge: $('#roleBadge'),
  authOutput: $('#authOutput'),
  profileOutput: $('#profileOutput'),
  premiumOutput: $('#premiumOutput'),
  purchaseOutput: $('#purchaseOutput'),
  claimOutput: $('#claimOutput'),
  adminOutput: $('#adminOutput'),
  productsTable: $('#productsTable'),
  myPoliciesTable: $('#myPoliciesTable'),
  adminPoliciesTable: $('#adminPoliciesTable'),
  myClaimsTable: $('#myClaimsTable'),
  claimDocumentsTable: $('#claimDocumentsTable'),
  adminClaimsTable: $('#adminClaimsTable'),
  statsGrid: $('#statsGrid'),
  policyReportOutput: $('#policyReportOutput'),
  claimsReportOutput: $('#claimsReportOutput'),
  revenueReportOutput: $('#revenueReportOutput'),
  auditLogsTable: $('#auditLogsTable')
};

elements.gatewayUrl.value = state.gatewayUrl;

function persistState() {
  localStorage.setItem('smartsureGatewayUrl', state.gatewayUrl);
  localStorage.setItem('smartsureToken', state.token);
  localStorage.setItem('smartsureRoles', JSON.stringify(state.roles));
  localStorage.setItem('smartsureUser', JSON.stringify(state.user));
}

function setBadge(element, text, className) {
  element.className = `badge ${className}`;
  element.textContent = text;
}

function updateSessionUi() {
  setBadge(elements.tokenBadge, state.token ? 'Stored' : 'None', state.token ? 'badge-ok' : 'badge-muted');
  setBadge(elements.roleBadge, state.roles[0] || 'Guest', state.roles.length ? 'badge-ok' : 'badge-muted');
}

function setConnection(text, kind) {
  setBadge(elements.connectionBadge, text, kind);
}

function serializeData(form) {
  return Object.fromEntries(new FormData(form).entries());
}

function showOutput(target, value) {
  if (!target) {
    return;
  }

  if (value == null) {
    target.textContent = '';
    return;
  }

  if (typeof value === 'string') {
    target.textContent = value;
    return;
  }

  target.textContent = JSON.stringify(value, null, 2);
}

function escapeHtml(value) {
  return String(value)
    .replaceAll('&', '&amp;')
    .replaceAll('<', '&lt;')
    .replaceAll('>', '&gt;')
    .replaceAll('"', '&quot;');
}

function formatValue(value) {
  if (value === null || value === undefined || value === '') {
    return '-';
  }

  if (Array.isArray(value)) {
    return value.join(', ');
  }

  if (typeof value === 'string') {
    const parsed = Date.parse(value);
    if (!Number.isNaN(parsed) && /T|-/u.test(value)) {
      return new Date(parsed).toLocaleString();
    }
  }

  return value;
}

function renderTable(target, rows, columns) {
  if (!target) {
    return;
  }

  if (!rows || rows.length === 0) {
    target.innerHTML = '<div class="response-box">No data loaded.</div>';
    return;
  }

  const header = columns.map((col) => `<th>${escapeHtml(col.label)}</th>`).join('');
  const body = rows.map((row) => {
    const cells = columns.map((col) => `<td>${escapeHtml(formatValue(row[col.key]))}</td>`).join('');
    return `<tr>${cells}</tr>`;
  }).join('');

  target.innerHTML = `<table><thead><tr>${header}</tr></thead><tbody>${body}</tbody></table>`;
}

async function request(path, options = {}) {
  const {
    method = 'GET',
    body,
    auth = true,
    responseType = 'json'
  } = options;

  const headers = {};
  if (!(body instanceof FormData) && body !== undefined) {
    headers['Content-Type'] = 'application/json';
  }

  if (auth && state.token) {
    headers.Authorization = `Bearer ${state.token}`;
  }

  const response = await fetch(`${state.gatewayUrl}${path}`, {
    method,
    headers,
    body: body instanceof FormData ? body : body === undefined ? undefined : JSON.stringify(body)
  });

  if (responseType === 'blob') {
    if (!response.ok) {
      throw new Error(`Request failed (${response.status})`);
    }

    return response.blob();
  }

  const contentType = response.headers.get('content-type') || '';
  const data = contentType.includes('application/json')
    ? await response.json()
    : await response.text();

  if (!response.ok) {
    const message = typeof data === 'string'
      ? data
      : data?.message || data?.title || `Request failed (${response.status})`;
    throw new Error(message);
  }

  return data;
}

async function pingGateway() {
  setConnection('Checking...', 'badge-warn');
  try {
    await request('/gateway/auth/google', { auth: false });
    setConnection('Connected', 'badge-ok');
  } catch (error) {
    setConnection('Offline', 'badge-warn');
    showOutput(elements.authOutput, error.message);
  }
}

function ensureAuthOutput(result) {
  showOutput(elements.authOutput, result);
}

function openGoogleLogin() {
  request('/gateway/auth/google', { auth: false })
    .then((url) => {
      if (typeof url === 'string') {
        window.open(url, '_blank', 'noopener,noreferrer');
      }
    })
    .catch((error) => ensureAuthOutput({ error: error.message }));
}

function saveGatewayUrl() {
  state.gatewayUrl = elements.gatewayUrl.value.trim() || 'http://localhost:5000';
  persistState();
  pingGateway();
}

function logout() {
  state.token = '';
  state.roles = [];
  state.user = null;
  persistState();
  updateSessionUi();
  showOutput(elements.authOutput, 'Logged out locally.');
}

async function loadProfile() {
  try {
    const profile = await request('/gateway/users/profile');
    state.user = profile;
    persistState();
    updateSessionUi();
    showOutput(elements.profileOutput, profile);

    const form = $('#profileForm');
    form.elements.fullName.value = profile.fullName || '';
    form.elements.phoneNumber.value = profile.phoneNumber || '';
    form.elements.address.value = profile.address || '';
  } catch (error) {
    showOutput(elements.profileOutput, { error: error.message });
  }
}

function setAuthFromResponse(result) {
  if (result?.token) {
    state.token = result.token;
    state.roles = result.roles || [];
    state.user = result;
    persistState();
    updateSessionUi();
  }
}

function bindForm(id, handler) {
  const form = $(id);
  form.addEventListener('submit', async (event) => {
    event.preventDefault();
    try {
      await handler(form);
    } catch (error) {
      const target = form.closest('.subcard')?.querySelector('.response-box');
      if (target) {
        showOutput(target, { error: error.message });
      } else {
        showOutput(elements.authOutput, { error: error.message });
      }
    }
  });
}

bindForm('#registerForm', async (form) => {
  const payload = serializeData(form);
  const result = await request('/gateway/auth/register', { method: 'POST', auth: false, body: payload });
  showOutput(elements.authOutput, result);
  $('#verifyOtpForm').elements.email.value = payload.email;
  $('#resendOtpForm').elements.email.value = payload.email;
});

bindForm('#verifyOtpForm', async (form) => {
  const payload = serializeData(form);
  const result = await request('/gateway/auth/register/verify-otp', { method: 'POST', auth: false, body: payload });
  setAuthFromResponse(result);
  showOutput(elements.authOutput, result);
});

bindForm('#resendOtpForm', async (form) => {
  const payload = serializeData(form);
  const result = await request('/gateway/auth/register/resend-otp', { method: 'POST', auth: false, body: payload });
  showOutput(elements.authOutput, result);
});

bindForm('#loginForm', async (form) => {
  const payload = serializeData(form);
  const result = await request('/gateway/auth/login', { method: 'POST', auth: false, body: payload });
  setAuthFromResponse(result);
  showOutput(elements.authOutput, result);
});

bindForm('#forgotForm', async (form) => {
  const payload = serializeData(form);
  const result = await request('/gateway/auth/forgot-password', { method: 'POST', auth: false, body: payload });
  showOutput(elements.authOutput, result || 'Reset OTP sent.');
});

bindForm('#resetForm', async (form) => {
  const payload = serializeData(form);
  const result = await request('/gateway/auth/reset-password', { method: 'POST', auth: false, body: payload });
  showOutput(elements.authOutput, result || 'Password reset complete.');
});

bindForm('#profileForm', async (form) => {
  const payload = serializeData(form);
  const result = await request('/gateway/users/profile', { method: 'PUT', body: payload });
  state.user = result;
  persistState();
  showOutput(elements.profileOutput, result);
});

bindForm('#premiumForm', async (form) => {
  const payload = serializeData(form);
  const result = await request(`/gateway/policies/calculate-premium?productId=${encodeURIComponent(payload.productId)}&coverageAmount=${encodeURIComponent(payload.coverageAmount)}&termMonths=${encodeURIComponent(payload.termMonths)}`);
  showOutput(elements.premiumOutput, result);
});

bindForm('#purchaseForm', async (form) => {
  const payload = serializeData(form);
  if (!payload.insuranceDate) {
    payload.insuranceDate = new Date().toISOString();
  }
  const result = await request('/gateway/policies/purchase', { method: 'POST', body: payload });
  showOutput(elements.purchaseOutput, result);
});

bindForm('#createProductForm', async (form) => {
  const payload = serializeData(form);
  payload.basePremium = Number(payload.basePremium);
  const result = await request('/gateway/policies/products', { method: 'POST', body: payload });
  showOutput(elements.purchaseOutput, result);
  await loadProducts();
});

bindForm('#updateProductForm', async (form) => {
  const payload = serializeData(form);
  const productId = payload.productId;
  delete payload.productId;
  payload.basePremium = Number(payload.basePremium);
  const result = await request(`/gateway/policies/products/${encodeURIComponent(productId)}`, { method: 'PUT', body: payload });
  showOutput(elements.purchaseOutput, result);
  await loadProducts();
});

bindForm('#deleteProductForm', async (form) => {
  const payload = serializeData(form);
  const result = await request(`/gateway/policies/products/${encodeURIComponent(payload.productId)}`, { method: 'DELETE', body: {} });
  showOutput(elements.purchaseOutput, result || 'Product deleted.');
  await loadProducts();
});

bindForm('#createClaimForm', async (form) => {
  const payload = serializeData(form);
  payload.claimAmount = Number(payload.claimAmount);
  const result = await request('/gateway/claims', { method: 'POST', body: payload });
  showOutput(elements.claimOutput, result);
  await loadMyClaims();
});

bindForm('#uploadDocumentForm', async (form) => {
  const fileInput = form.elements.fileContent;
  const file = fileInput.files?.[0];
  if (!file) {
    throw new Error('Select a file first.');
  }

  const buffer = await file.arrayBuffer();
  const bytes = new Uint8Array(buffer);
  let binary = '';
  bytes.forEach((byte) => {
    binary += String.fromCharCode(byte);
  });

  const payload = {
    fileName: form.elements.fileName.value,
    fileType: form.elements.fileType.value,
    fileSizeKb: Number(form.elements.fileSizeKb.value),
    contentBase64: btoa(binary)
  };

  const claimId = form.elements.claimId.value;
  const result = await request(`/gateway/claims/${encodeURIComponent(claimId)}/documents`, { method: 'POST', body: payload });
  showOutput(elements.claimOutput, result);
});

bindForm('#loadDocumentsForm', async (form) => {
  const { claimId } = serializeData(form);
  const result = await request(`/gateway/claims/${encodeURIComponent(claimId)}/documents`);
  renderTable(elements.claimDocumentsTable, result, [
    { key: 'docId', label: 'Doc ID' },
    { key: 'fileName', label: 'File' },
    { key: 'fileType', label: 'Type' },
    { key: 'fileSizeKb', label: 'Size KB' },
    { key: 'fileUrl', label: 'File URL' }
  ]);
});

bindForm('#claimReviewForm', async (form) => {
  const payload = serializeData(form);
  const claimId = payload.claimId;
  delete payload.claimId;
  const result = await request(`/gateway/claims/admin/${encodeURIComponent(claimId)}/review`, { method: 'PUT', body: payload });
  showOutput(elements.claimOutput, result);
});

bindForm('#claimApproveForm', async (form) => {
  const payload = serializeData(form);
  const claimId = payload.claimId;
  delete payload.claimId;
  if (payload.approvedAmount === '') {
    payload.approvedAmount = null;
  } else {
    payload.approvedAmount = Number(payload.approvedAmount);
  }
  const result = await request(`/gateway/claims/admin/${encodeURIComponent(claimId)}/approve`, { method: 'PUT', body: payload });
  showOutput(elements.claimOutput, result);
});

bindForm('#claimRejectForm', async (form) => {
  const payload = serializeData(form);
  const claimId = payload.claimId;
  delete payload.claimId;
  const result = await request(`/gateway/claims/admin/${encodeURIComponent(claimId)}/reject`, { method: 'PUT', body: payload });
  showOutput(elements.claimOutput, result);
});

bindForm('#policyReportForm', async (form) => {
  const payload = serializeData(form);
  const query = new URLSearchParams();
  if (payload.from) query.set('from', payload.from);
  if (payload.to) query.set('to', payload.to);
  if (payload.type) query.set('typeFilter', payload.type);
  const result = await request(`/gateway/admin/reports/policies?${query.toString()}`);
  showOutput(elements.policyReportOutput, result);
});

bindForm('#claimsReportForm', async (form) => {
  const payload = serializeData(form);
  const query = new URLSearchParams();
  if (payload.from) query.set('from', payload.from);
  if (payload.to) query.set('to', payload.to);
  if (payload.status) query.set('statusFilter', payload.status);
  const result = await request(`/gateway/admin/reports/claims?${query.toString()}`);
  showOutput(elements.claimsReportOutput, result);
});

bindForm('#revenueReportForm', async (form) => {
  const payload = serializeData(form);
  const query = new URLSearchParams();
  if (payload.from) query.set('from', payload.from);
  if (payload.to) query.set('to', payload.to);
  const result = await request(`/gateway/admin/reports/revenue?${query.toString()}`);
  showOutput(elements.revenueReportOutput, result);
});

bindForm('#auditLogsForm', async (form) => {
  const payload = serializeData(form);
  const query = new URLSearchParams();
  if (payload.from) query.set('from', payload.from);
  if (payload.to) query.set('to', payload.to);
  if (payload.action) query.set('action', payload.action);
  if (payload.entityType) query.set('entityType', payload.entityType);
  query.set('page', payload.page || '1');
  query.set('pageSize', payload.pageSize || '20');
  const result = await request(`/gateway/admin/audit-logs?${query.toString()}`);
  renderTable(elements.auditLogsTable, result.items || [], [
    { key: 'logId', label: 'Log ID' },
    { key: 'action', label: 'Action' },
    { key: 'entityType', label: 'Entity' },
    { key: 'entityId', label: 'Entity ID' },
    { key: 'timeStamp', label: 'Time' }
  ]);
});

bindForm('#exportReportForm', async (form) => {
  const { reportId } = serializeData(form);
  const blob = await request(`/gateway/admin/reports/export?reportId=${encodeURIComponent(reportId)}`, { responseType: 'blob' });
  const url = URL.createObjectURL(blob);
  const anchor = document.createElement('a');
  anchor.href = url;
  anchor.download = `smartsure-report-${reportId}.pdf`;
  anchor.click();
  URL.revokeObjectURL(url);
  showOutput(elements.adminOutput, { message: 'PDF download started.' });
});

async function loadProducts() {
  const result = await request('/gateway/policies/products', { auth: false });
  renderTable(elements.productsTable, result, [
    { key: 'productId', label: 'Product ID' },
    { key: 'typeName', label: 'Type' },
    { key: 'subTypeName', label: 'Sub Type' },
    { key: 'basePremium', label: 'Base Premium' }
  ]);
}

async function loadMyPolicies() {
  const result = await request('/gateway/policies/my-policies');
  renderTable(elements.myPoliciesTable, result, [
    { key: 'policyId', label: 'Policy ID' },
    { key: 'policyNumber', label: 'Policy No.' },
    { key: 'typeName', label: 'Type' },
    { key: 'subTypeName', label: 'Sub Type' },
    { key: 'monthlyPremium', label: 'Monthly Premium' },
    { key: 'status', label: 'Status' }
  ]);
}

async function loadAdminPolicies() {
  const result = await request('/gateway/policies/admin/all');
  renderTable(elements.adminPoliciesTable, result, [
    { key: 'policyId', label: 'Policy ID' },
    { key: 'policyNumber', label: 'Policy No.' },
    { key: 'userId', label: 'User ID' },
    { key: 'status', label: 'Status' },
    { key: 'coverageAmount', label: 'Coverage' }
  ]);
}

async function loadMyClaims() {
  const result = await request('/gateway/claims/my-claims');
  renderTable(elements.myClaimsTable, result, [
    { key: 'claimId', label: 'Claim ID' },
    { key: 'claimNumber', label: 'Claim No.' },
    { key: 'policyId', label: 'Policy ID' },
    { key: 'status', label: 'Status' },
    { key: 'claimAmount', label: 'Amount' }
  ]);
}

async function loadAdminClaims() {
  const result = await request('/gateway/claims/admin/all');
  renderTable(elements.adminClaimsTable, result, [
    { key: 'claimId', label: 'Claim ID' },
    { key: 'claimNumber', label: 'Claim No.' },
    { key: 'userId', label: 'User ID' },
    { key: 'status', label: 'Status' },
    { key: 'claimAmount', label: 'Amount' }
  ]);
}

async function loadDashboard() {
  const result = await request('/gateway/admin/dashboard/stats');
  const items = [
    { value: result.totalPolicies, name: 'Policies' },
    { value: result.totalClaims, name: 'Claims' },
    { value: result.totalRevenue, name: 'Revenue' }
  ];
  elements.statsGrid.innerHTML = items.map((item) => `
    <article class="metric">
      <span class="value">${escapeHtml(item.value)}</span>
      <span class="name">${escapeHtml(item.name)}</span>
    </article>
  `).join('');
}

function wireButtons() {
  $('#saveGatewayBtn').addEventListener('click', saveGatewayUrl);
  $('#testConnectionBtn').addEventListener('click', pingGateway);
  $('#connectQuickBtn').addEventListener('click', pingGateway);
  $('#googleLoginBtn').addEventListener('click', openGoogleLogin);
  $('#loadProfileBtn').addEventListener('click', loadProfile);
  $('#logoutBtn').addEventListener('click', logout);
  $('#loadProductsBtn').addEventListener('click', loadProducts);
  $('#loadMyPoliciesBtn').addEventListener('click', loadMyPolicies);
  $('#loadAdminPoliciesBtn').addEventListener('click', loadAdminPolicies);
  $('#loadMyClaimsBtn').addEventListener('click', loadMyClaims);
  $('#loadAdminClaimsBtn').addEventListener('click', loadAdminClaims);
  $('#loadStatsBtn').addEventListener('click', loadDashboard);
}

wireButtons();
updateSessionUi();

Promise.allSettled([
  pingGateway(),
  loadProducts(),
  state.token ? loadProfile() : Promise.resolve(),
  state.token ? loadMyPolicies() : Promise.resolve(),
  state.token ? loadMyClaims() : Promise.resolve(),
  state.token ? loadDashboard() : Promise.resolve()
]);
