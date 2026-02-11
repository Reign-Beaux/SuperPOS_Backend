using Domain.Entities.Roles;
using Domain.Entities.Sales;
using Domain.Exceptions;
using Domain.ValueObjects;

namespace Domain.Entities.Users;

/// <summary>
/// User aggregate root representing a system user with authentication and authorization.
/// </summary>
public class User : BaseEntity, IAggregateRoot
{
    // Backing fields for value objects
    private PersonName? _name;
    private Email? _email;
    private PhoneNumber? _phone;

    // Parameterless constructor required by EF Core
    public User() { }

    // Primitive properties for EF Core persistence
    public string Name { get; set; } = string.Empty;
    public string FirstLastname { get; set; } = string.Empty;
    public string? SecondLastname { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHashed { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public Guid RoleId { get; set; }

    // Authentication and security fields
    public bool IsActive { get; set; } = true;
    public DateTime? LastLoginAt { get; set; }
    public int FailedLoginAttempts { get; set; } = 0;
    public DateTime? LockedUntilAt { get; set; }

    // Computed property for account lockout status
    public bool IsLocked => LockedUntilAt.HasValue && LockedUntilAt.Value > DateTime.UtcNow;

    // Value Object properties for domain logic
    public PersonName NameValue
    {
        get => _name ??= PersonName.Create(Name, FirstLastname, SecondLastname);
        private set => _name = value;
    }

    public Email EmailValue
    {
        get => _email ??= ValueObjects.Email.Create(Email);
        private set => _email = value;
    }

    public PhoneNumber? PhoneValue
    {
        get => _phone ??= Phone is not null ? ValueObjects.PhoneNumber.Create(Phone) : null;
        private set => _phone = value;
    }

    // Navigation Properties
    public ICollection<Sale> Sales { get; set; } = [];
    public Role Role { get; set; } = null!;

    /// <summary>
    /// Factory method to create a new User with valid values.
    /// Password must be already hashed before calling this method.
    /// </summary>
    public static User Create(
        PersonName name,
        Email email,
        string hashedPassword,
        Guid roleId,
        PhoneNumber? phone = null)
    {
        if (string.IsNullOrWhiteSpace(hashedPassword))
            throw new BusinessRuleViolationException("USER_001", "Password hash cannot be empty");

        if (hashedPassword.Length < 10)
            throw new BusinessRuleViolationException("USER_002", "Invalid password hash format");

        if (roleId == Guid.Empty)
            throw new BusinessRuleViolationException("USER_003", "RoleId cannot be empty");

        return new User
        {
            Name = name.FirstName,
            FirstLastname = name.FirstLastname,
            SecondLastname = name.SecondLastname,
            Email = email.Value,
            PasswordHashed = hashedPassword,
            Phone = phone?.Value,
            RoleId = roleId,
            _name = name,
            _email = email,
            _phone = phone
        };
    }

    /// <summary>
    /// Updates the user's name.
    /// </summary>
    public void UpdateName(PersonName name)
    {
        Name = name.FirstName;
        FirstLastname = name.FirstLastname;
        SecondLastname = name.SecondLastname;
        _name = name;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the user's email address.
    /// </summary>
    public void UpdateEmail(Email email)
    {
        Email = email.Value;
        _email = email;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the user's phone number.
    /// </summary>
    public void UpdatePhone(PhoneNumber? phone)
    {
        Phone = phone?.Value;
        _phone = phone;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates all user information except password and email.
    /// Email requires separate method for security reasons.
    /// </summary>
    public void UpdateInfo(PersonName name, PhoneNumber? phone)
    {
        UpdateName(name);
        UpdatePhone(phone);
    }

    /// <summary>
    /// Changes the user's password.
    /// Password must be already hashed before calling this method.
    /// </summary>
    public void ChangePassword(string newHashedPassword)
    {
        if (string.IsNullOrWhiteSpace(newHashedPassword))
            throw new BusinessRuleViolationException("USER_001", "Password hash cannot be empty");

        if (newHashedPassword.Length < 10)
            throw new BusinessRuleViolationException("USER_002", "Invalid password hash format");

        if (PasswordHashed == newHashedPassword)
            return; // No change needed

        PasswordHashed = newHashedPassword;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Verifies if the provided hashed password matches the user's password.
    /// OBSOLETE: Use IEncryptionService.VerifyText() instead for proper BCrypt verification.
    /// This method uses simple string comparison and should not be used for authentication.
    /// </summary>
    [Obsolete("Use IEncryptionService.VerifyText() for proper BCrypt password verification")]
    public bool VerifyPassword(string hashedPassword)
    {
        return PasswordHashed == hashedPassword;
    }

    /// <summary>
    /// Gets the user's full name.
    /// </summary>
    public string GetFullName() => NameValue.GetFullName();

    /// <summary>
    /// Gets the user's formal name (Last names, First name).
    /// </summary>
    public string GetFormalName() => NameValue.GetFormalName();

    /// <summary>
    /// Checks if the user has a phone number.
    /// </summary>
    public bool HasPhone() => !string.IsNullOrWhiteSpace(Phone);

    /// <summary>
    /// Updates the user's role.
    /// </summary>
    public void UpdateRole(Guid roleId)
    {
        if (roleId == Guid.Empty)
            throw new BusinessRuleViolationException("USER_003", "RoleId cannot be empty");

        if (RoleId == roleId)
            return; // No change needed

        RoleId = roleId;
        UpdatedAt = DateTime.UtcNow;
    }

    // Authentication and account management methods

    /// <summary>
    /// Records a successful login attempt.
    /// Resets failed login attempts and updates last login timestamp.
    /// </summary>
    public void RecordSuccessfulLogin()
    {
        LastLoginAt = DateTime.UtcNow;
        FailedLoginAttempts = 0;
        LockedUntilAt = null;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Records a failed login attempt.
    /// Locks the account if max attempts is reached.
    /// </summary>
    /// <param name="maxAttempts">Maximum failed attempts before lockout (default: 5)</param>
    /// <param name="lockoutMinutes">Duration of lockout in minutes (default: 30)</param>
    public void RecordFailedLogin(int maxAttempts = 5, int lockoutMinutes = 30)
    {
        FailedLoginAttempts++;
        UpdatedAt = DateTime.UtcNow;

        if (FailedLoginAttempts >= maxAttempts)
        {
            LockedUntilAt = DateTime.UtcNow.AddMinutes(lockoutMinutes);
        }
    }

    /// <summary>
    /// Manually unlocks a locked account.
    /// Resets failed login attempts and clears lockout timestamp.
    /// </summary>
    public void Unlock()
    {
        LockedUntilAt = null;
        FailedLoginAttempts = 0;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Deactivates the user account.
    /// Prevents login without deleting the account.
    /// </summary>
    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Activates a deactivated user account.
    /// Resets lockout and failed attempts.
    /// </summary>
    public void Activate()
    {
        IsActive = true;
        FailedLoginAttempts = 0;
        LockedUntilAt = null;
        UpdatedAt = DateTime.UtcNow;
    }
}
