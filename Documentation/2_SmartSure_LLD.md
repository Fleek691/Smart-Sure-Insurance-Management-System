# SmartSure Platform  Low-Level Design (LLD)

**Version:** 2.0

---

## 1. Entity Relationship Diagrams

### 1.1 IdentityService — AuthDb

```
┌──────────────────────────────────────────────────────
  Users                                                                        
   
  UserId          UNIQUEIDENTIFIER  PK                                         
  FullName        NVARCHAR(150)     NOT NULL                                   
  Email           NVARCHAR(256)     NOT NULL  UNIQUE                           
  PhoneNumber     NVARCHAR(20)      NULL                                       
  Address         NVARCHAR(300)     NULL                                       
  IsActive        BIT               NOT NULL  DEFAULT 1                        
  GoogleSubject   NVARCHAR(MAX)     NULL                                       
  CreatedAt       DATETIME2         NOT NULL                                   
  LastLoginAt     DATETIME2         NULL                                       
  RefreshToken    NVARCHAR(MAX)     NULL                                       
  RefreshTokenExpiryTime DATETIME2  NULL                                       

          1                                     1
                                               
          0..1                                  0..*
              
  Passwords                         PasswordResetTokens  
                    
  PassId  PK                        Id        PK         
  UserId  FKUsers                  UserId    FKUsers   
  PasswordHash                      Token     NVARCHAR   
                ExpiresAt DATETIME2  
                                      IsUsed    BIT        
                                    
          1
         
          0..*

  UserRoles         
    
  Id      PK        
  UserId  FKUsers  
  RoleId  FKRoles  

          0..*
         
          1

  Roles             
    
  RoleId   PK       
  RoleName UNIQUE   
  (CUSTOMER, ADMIN) 

```

### 1.2 PolicyService  PolicyDb

```

  InsuranceTypes                                                               
  TypeId    INT  PK  IDENTITY                                                  
  TypeName  NVARCHAR(100)  UNIQUE  (e.g. "Vehicle", "Home")                   

          1
         
          1..*

  InsuranceSubTypes                                                            
  SubTypeId   INT  PK  IDENTITY                                                
  TypeId      FKInsuranceTypes                                                
  SubTypeName NVARCHAR(150)  (e.g. "Maruti Suzuki", "Apartment")              
  BasePremium DECIMAL(18,2)  (annual base premium in INR)                     
  UNIQUE(TypeId, SubTypeName)                                                  
─
          1
         
          0..*
─
  Policies                                                                     
  PolicyId       UNIQUEIDENTIFIER  PK  DEFAULT NEWID()                        
  PolicyNumber   NVARCHAR(25)  UNIQUE  (e.g. SS-20260414-4821)                
  UserId         UNIQUEIDENTIFIER  (FK to AuthDb.Users  cross-service ref)   
  TypeId         FKInsuranceTypes                                             
  SubTypeId      FKInsuranceSubTypes                                          
  Status         NVARCHAR(20)  (ACTIVE, CANCELLED, EXPIRED, DRAFT)            
  IssuedDate     DATETIME2                                                     
  InsuranceDate  DATE                                                          
  EndDate        DATE                                                          
  CoverageAmount DECIMAL(14,2)                                                 
  MonthlyPremium DECIMAL(10,2)                                                 
  CreatedAt      DATETIME2                                                     
  UpdatedAt      DATETIME2                                                     

     1           1           1           1           1
                                                    
     0..1        0..1        0..1        0..*        0..*
    
PolicyDet. VehicleDet HomeDet.   Premiums   Payments  
    
PolicyDet  VehicleId  HomeId     PremiumId  PaymentId 
 ailId PK  PK         PK         PK         PK        
PolicyId   PolicyId   PolicyId   PolicyId   PolicyId  
FK         FK         FK         FK         FK        
Details    VehicleNum Address    Amount     Amount    
(JSON)     Model      YearBuilt  DueDate    PaymentDt 
           YearOfMake Construct  PaidAt     Method    
           EngineCC   ionType    Status     TxnRef    
    Status    
                                                      
```

### 1.3 ClaimsService  ClaimsDb

```

  Claims                                                                       
  ClaimId      UNIQUEIDENTIFIER  PK  DEFAULT NEWID()                          
  ClaimNumber  NVARCHAR(25)  UNIQUE  (e.g. CLM-20260414-4821)                 
  PolicyId     UNIQUEIDENTIFIER  (cross-service ref to PolicyDb.Policies)     
  UserId       UNIQUEIDENTIFIER  (cross-service ref to AuthDb.Users)          
  Description  NVARCHAR(1000)                                                  
  ClaimAmount  DECIMAL(14,2)                                                   
  Status       NVARCHAR(30)  (DRAFT, SUBMITTED, UNDER_REVIEW, APPROVED, REJECTED)
  AdminNote    NVARCHAR(500)  NULL                                             
  ReviewedBy   UNIQUEIDENTIFIER  NULL                                          
  CreatedDate  DATETIME2                                                       
  SubmittedAt  DATETIME2  NULL                                                 
  ReviewedAt   DATETIME2  NULL                                                 
  UpdatedAt    DATETIME2                                                       

          1                                     1
                                               
          0..*                                  0..*
    
  ClaimDocuments                  ClaimStatusHistory             
          
  DocId       PK                  HistoryId  PK                  
  ClaimId     FKClaims           ClaimId    FKClaims           
  FileName    NVARCHAR(255)       OldStatus  NVARCHAR(30)        
  MegaNzFileId NVARCHAR(500)      NewStatus  NVARCHAR(30)        
  FileUrl     NVARCHAR(1000)      ChangedBy  UNIQUEIDENTIFIER    
  FileType    NVARCHAR(50)        ChangedDate DATETIME2          
  FileSizeKb  INT                 Note       NVARCHAR(300) NULL  
  UploadedAt  DATETIME2         

```

### 1.4 AdminService  AdminDb

```

  AuditLogs                                                                    
  LogId      UNIQUEIDENTIFIER  PK  DEFAULT NEWID()                            
  UserId     UNIQUEIDENTIFIER  (actor who triggered the event)                
  Action     NVARCHAR(100)  (USER_REGISTERED, POLICY_ACTIVATED, etc.)         
  EntityType NVARCHAR(50)   (User, Policy, Claim)                             
  EntityId   NVARCHAR(50)   (string PK of the entity)                         
  Details    NVARCHAR(MAX)  NULL  (JSON event payload)                        
  TimeStamp  DATETIME2                                                         
  INDEX(TimeStamp), INDEX(EntityType), INDEX(Action), INDEX(UserId)           



  Reports                                                                      
  ReportId      UNIQUEIDENTIFIER  PK  DEFAULT NEWID()                         
  ReportType    NVARCHAR(50)  (POLICY, CLAIM, REVENUE)                        
  GeneratedDate DATETIME2                                                      
  UserId        UNIQUEIDENTIFIER  (admin who generated it)                    
  PeriodFrom    DATE  NULL                                                     
  PeriodTo      DATE  NULL                                                     
  DataJson      NVARCHAR(MAX)  (serialized report DTO)                        
  INDEX(ReportType), INDEX(GeneratedDate), INDEX(UserId)                      

```

---

## 2. Class Diagrams

### 2.1 IdentityService

```

  AuthController                                                              
    
  - _authService: IAuthService                                                
  - _googleAuthService: IGoogleAuthService                                    
  + Register(RegisterDto) : OtpDispatchResponseDto                            
  + VerifyRegistrationOtp(VerifyRegistrationOtpDto) : AuthResponseDto         
  + ResendRegistrationOtp(ResendRegistrationOtpDto) : OtpDispatchResponseDto  
  + Login(LoginDto) : AuthResponseDto                                         
  + GoogleLoginUrl() : string                                                 
  + GoogleCallback() : IActionResult                                          
  + RefreshToken(RefreshTokenDto) : AuthResponseDto                           
  + ForgotPassword(ForgotPasswordDto) : IActionResult                         
  + ResetPassword(ResetPasswordDto) : IActionResult                           

                     uses                           uses
                                                   
   
  AuthService                      GoogleAuthService                   
          
  - _memoryCache                   - _userRepository                   
  - _userRepository                - _roleRepository                   
  - _roleRepository                - _jwtService                       
  - _jwtService                    - _eventPublisher                   
  - _emailService                  - _memoryCache                      
  - _eventPublisher                - _httpClient                       
  + RegisterAsync()                + GetGoogleLoginUrl() : string      
  + VerifyRegistrationOtpAsync     + ProcessGoogleCallbackAsync()      
  + ResendRegistrationOtpAsync     - ValidateGoogleTokenAsync()        
  + LoginAsync()                   - SignInGoogleUserAsync()           
  + RefreshTokenAsync()            - ExchangeGoogleAuthCodeAsync()     
  + RequestPasswordResetAsync    
  + ResetPasswordAsync()      
  - GenerateOtp() : string    
  - CacheRegistrationOtp()    
  - GetJwtAudiences()         

                     uses
                    
   
  UserRepository                   JwtService                  
         
  + GetByEmailAsync()              + BuildToken() : string     
  + GetByIdAsync()                 + GenerateRefreshToken()    
  + GetByRefreshTokenAsync()         : string                  
  + GetAllAsync()                
  + AddAsync()                
  + SaveChangesAsync()        

```

### 2.2 PolicyService

```

  PoliciesController                                                          
  + GetProducts() + GetProductById() + CreateProduct() + UpdateProduct()      
  + DeleteProduct() + CalculatePremium() + Purchase()                         
  + CreatePaymentOrder() + VerifyPayment()                                    
  + GetMyPolicies() + GetById() + Cancel()                                    
  + GetAllForAdmin() + UpdatePolicyStatus()                                   

               uses                                     uses
                                                       
   
  PolicyService                        RazorpayService                     
          
  - _policyRepository                  - _policyRepository                 
  - _memoryCache                       - _memoryCache                      
  - _eventPublisher                    - _eventPublisher                   
  - _logger                            - _logger                           
  + GetProductsAsync()                 + CreateOrderAsync()                
  + GetProductByIdAsync()              + VerifyAndActivateAsync()          
  + CreateProductAsync()               - ValidatePurchaseInput()           
  + UpdateProductAsync()               - CalculatePremium()                
  + DeleteProductAsync()               - VerifySignature() HMAC-SHA256     
  + CalculatePremiumAsync()            PendingPurchase (inner class)       
  + PurchasePolicyAsync()            
  + GetMyPoliciesAsync()          
  + GetPolicyByIdAsync()          
  + CancelPolicyAsync()           
  + GetAllPoliciesForAdminAsync() 
  + UpdatePolicyStatusAsync()     

               uses
              

  PolicyRepository                
  + GetProductsAsync()            
  + GetProductByIdAsync()         
  + GetTypeByNameAsync()          
  + AddTypeAsync()                
  + AddProductAsync()             
  + RemoveProductAsync()          
  + GetByIdAsync()                
  + GetByUserIdAsync()            
  + GetAllAsync()                 
  + AddAsync()                    
  + AddPaymentAsync()             
  + SaveChangesAsync()            

```

### 2.3 ClaimsService

```

  ClaimsController                                                            
  + Create() + GetMyClaims() + GetById() + Submit()                           
  + UploadDocument() + GetDocuments() + DeleteDocument()                      
  + GetAllForAdmin() + SetUnderReview() + Approve() + Reject()                

               uses                                     uses
                                                       
   
  ClaimService                         ClaimAdminService                   
          
  - _claimRepository                   - _claimRepository                  
  - _eventPublisher                    - _eventPublisher                   
  - _memoryCache                       - _memoryCache                      
  - _megaStorageService                + GetAllClaimsForAdminAsync()       
  - _policyVerification                + MarkUnderReviewAsync()            
  + CreateClaimAsync()                 + ApproveClaimAsync()               
  + GetMyClaimsAsync()                 + RejectClaimAsync()                
  + GetClaimAsync()                    - UpdateClaimStatusByAdminAsync()   
  + SubmitClaimAsync()                   (state machine enforcer)          
  + UploadDocumentAsync()            
  + GetDocumentsAsync()           
  + DeleteDocumentAsync()         
  - ParseBase64()                 

               uses
              
   
  ClaimRepository                      MegaStorageService                  
  + GetByIdAsync()                     + UploadAsync()                     
  + GetByUserIdAsync()                 - UploadToMegaAsync() [15s timeout] 
  + GetAllAsync()                      - StoreLocally() [fallback]         
  + AddAsync()                       
  + GetDocumentsAsync()           
  + GetDocumentByIdAsync()           
  + AddDocumentAsync()                 PolicyVerificationService           
  + RemoveDocumentAsync()              + GetPolicyStatusAsync()            
  + AddStatusHistoryAsync()            (HTTP GET to PolicyService)         
  + SaveChangesAsync()               

```

### 2.4 AdminService

```

  AdminController                                                             
  + GetDashboardStats() + GetPolicyReport() + GetClaimsReport()               
  + GetRevenueReport() + ExportReport() + GetAuditLogs()                      

               uses
              

  AdminService                    
  - _adminRepository              
  - _memoryCache                  
  + GetDashboardStatsAsync()      
  + GetPolicyReportAsync()        
  + GetClaimsReportAsync()        
  + GetRevenueReportAsync()       
  + ExportReportPdfAsync()        
  + GetAuditLogsAsync()           
  - SaveReportAsync()             
  - BuildPdf()                    
  - CreateSimplePdf()             
  - ExtractAmount()               

               uses
              
   
  AdminRepository                      AuditEventConsumer                  
  + CountDistinctEntitiesAsync()       Implements IConsumer for:           
  + SumAmountFromAuditDetailsAsync     - UserRegisteredEvent               
  + GetAuditLogsAsync()                - PolicyActivatedEvent              
  + GetAuditLogsCountAsync()           - PolicyCancelledEvent              
  + GetPolicyLogsAsync()               - ClaimSubmittedEvent               
  + GetClaimLogsAsync()                - ClaimStatusChangedEvent           
  + GetRevenueLogsAsync()              - ClaimApprovedEvent                
  + AddReportAsync()                   - ClaimRejectedEvent                
  + GetReportByIdAsync()               - SaveAuditLogAsync() [all events]  
  + SaveChangesAsync()               
  - BuildAuditQuery()             
  - ExtractAmount()               
  - TryGetDecimal()               

```

