namespace BankApp.BankApp.Common;

public enum ResultErrorCode
{
    ValidationError,
    NotFound,
    Unauthorized,
    Forbidden,
    Conflict,
    TooManyRequests,
    DatabaseError,
    DuplicateEmail,
    InvalidCredentials,
    AccountInactive,
    InsufficientBalance,
    InvalidTransaction,
    BillAlreadyPaid
}
