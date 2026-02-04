using Domain.Entities.Sales;
using Domain.Exceptions;
using Domain.ValueObjects;

namespace Domain.Entities.Customers;

/// <summary>
/// Customer aggregate root representing a person who purchases products.
/// </summary>
public class Customer : BaseEntity, IAggregateRoot
{
    // Backing fields for value objects
    private PersonName? _name;
    private Email? _email;
    private PhoneNumber? _phone;

    // Parameterless constructor required by EF Core
    public Customer() { }

    // Primitive properties for EF Core persistence
    public string Name { get; set; } = string.Empty;
    public string FirstLastname { get; set; } = string.Empty;
    public string? SecondLastname { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public DateTime? BirthDate { get; set; }

    // Value Object properties for domain logic
    public PersonName NameValue
    {
        get => _name ??= PersonName.Create(Name, FirstLastname, SecondLastname);
        private set => _name = value;
    }

    public Email? EmailValue
    {
        get => _email ??= Email is not null ? ValueObjects.Email.Create(Email) : null;
        private set => _email = value;
    }

    public PhoneNumber? PhoneValue
    {
        get => _phone ??= Phone is not null ? ValueObjects.PhoneNumber.Create(Phone) : null;
        private set => _phone = value;
    }

    // Navigation Properties
    public ICollection<Sale> Sales { get; set; } = [];

    /// <summary>
    /// Factory method to create a new Customer with valid values.
    /// </summary>
    public static Customer Create(
        PersonName name,
        Email? email = null,
        PhoneNumber? phone = null,
        DateTime? birthDate = null)
    {
        return new Customer
        {
            Name = name.FirstName,
            FirstLastname = name.FirstLastname,
            SecondLastname = name.SecondLastname,
            Email = email?.Value,
            Phone = phone?.Value,
            BirthDate = birthDate,
            _name = name,
            _email = email,
            _phone = phone
        };
    }

    /// <summary>
    /// Updates the customer's name.
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
    /// Updates the customer's contact information.
    /// </summary>
    public void UpdateContactInfo(Email? email, PhoneNumber? phone)
    {
        Email = email?.Value;
        Phone = phone?.Value;
        _email = email;
        _phone = phone;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates the customer's birth date.
    /// </summary>
    public void UpdateBirthDate(DateTime? birthDate)
    {
        if (birthDate.HasValue && birthDate.Value > DateTime.UtcNow)
            throw new BusinessRuleViolationException("CUSTOMER_001", "Birth date cannot be in the future");

        BirthDate = birthDate;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Updates all customer information at once.
    /// </summary>
    public void UpdateInfo(PersonName name, Email? email, PhoneNumber? phone, DateTime? birthDate)
    {
        UpdateName(name);
        UpdateContactInfo(email, phone);
        UpdateBirthDate(birthDate);
    }

    /// <summary>
    /// Calculates the customer's age based on their birth date.
    /// </summary>
    public int? GetAge()
    {
        if (!BirthDate.HasValue)
            return null;

        var today = DateTime.UtcNow;
        var age = today.Year - BirthDate.Value.Year;

        // Adjust if birthday hasn't occurred this year yet
        if (today.Month < BirthDate.Value.Month ||
            (today.Month == BirthDate.Value.Month && today.Day < BirthDate.Value.Day))
        {
            age--;
        }

        return age;
    }

    /// <summary>
    /// Gets the customer's full name.
    /// </summary>
    public string GetFullName() => NameValue.GetFullName();

    /// <summary>
    /// Gets the customer's formal name (Last names, First name).
    /// </summary>
    public string GetFormalName() => NameValue.GetFormalName();

    /// <summary>
    /// Checks if the customer has an email address.
    /// </summary>
    public bool HasEmail() => !string.IsNullOrWhiteSpace(Email);

    /// <summary>
    /// Checks if the customer has a phone number.
    /// </summary>
    public bool HasPhone() => !string.IsNullOrWhiteSpace(Phone);

    /// <summary>
    /// Checks if the customer has any contact information.
    /// </summary>
    public bool HasContactInfo() => HasEmail() || HasPhone();

    /// <summary>
    /// Checks if the customer is an adult (18 years or older).
    /// </summary>
    public bool IsAdult() => GetAge() >= 18;
}
