using Application.UseCases.Sales.DTOs;

namespace Application.UseCases.CashRegisters.DTOs;

/// <summary>
/// Complete cash register closing report.
/// Includes the closing record and all sales from the period.
/// </summary>
public record CashRegisterReportDTO(
    CashRegisterDTO CashRegister,
    List<SaleDTO> Sales
);
