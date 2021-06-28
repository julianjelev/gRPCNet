using gRPCNet.ServerAPI.Constants.Cards;
using gRPCNet.ServerAPI.Models.Domain.Common;
using gRPCNet.ServerAPI.Models.Domain.Owners;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace gRPCNet.ServerAPI.Models.Domain.Cards
{
    public class Card
    {
        public Card()
        {
            Currencies = new List<Currency>();
        }

        [Key]
        public string Id { get; set; }
        public DateTime ActivatedOn { get; set; }
        public DateTime CreatedOn { get; set; }
        public int DaysToLive { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        [MaxLength(100)]
        public string Number { get; set; }
        [ForeignKey("Owner")]
        public string OwnerId { get; set; }
        [MaxLength(100)]
        public string Serie { get; set; }
        public string UserId { get; set; }
        public DateTime ExpirationDate { get; set; }
        [ForeignKey("CardMode")]
        public string CardModeId { get; set; }
        [ForeignKey("CardState")]
        public string CardStateId { get; set; }
        public string ExternalId { get; set; }
        public DateTime LastTransactionTime { get; set; }
        public int MinTransactionTime { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
        public string AnnulledInfo { get; set; }
        public bool FreePeriod { get; set; }
        public DateTime? FreePeriodEnd { get; set; }
        public DateTime? FreePeriodStart { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Currency1Balance { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Currency1BalanceBonus { get; set; }
        public string Currency1Code { get; set; }
        public string Currency1Name { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Currency1TotalIncome { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Currency1TotalSpend { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CurrencyBonusBalance { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal CurrencyBonusBalanceBonus { get; set; }
        public string CurrencyBonusCode { get; set; }
        public string CurrencyBonusName { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal CurrencyBonusTotalIncome { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal CurrencyBonusTotalSpend { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CurrencyCreditBalance { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal CurrencyCreditBalanceBonus { get; set; }
        public string CurrencyCreditCode { get; set; }
        public string CurrencyCreditName { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal CurrencyCreditTotalIncome { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal CurrencyCreditTotalSpend { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CurrencyPrimaryBalance { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal CurrencyPrimaryBalanceBonus { get; set; }
        public string CurrencyPrimaryCode { get; set; }
        public string CurrencyPrimaryName { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal CurrencyPrimaryTotalIncome { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal CurrencyPrimaryTotalSpend { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CurrencyTicketBalance { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal CurrencyTicketBalanceBonus { get; set; }
        public string CurrencyTicketCode { get; set; }
        public string CurrencyTicketName { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal CurrencyTicketTotalIncome { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal CurrencyTicketTotalSpend { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal CurrencyTimeBalance { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal CurrencyTimeBalanceBonus { get; set; }
        public string CurrencyTimeCode { get; set; }
        public string CurrencyTimeName { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal CurrencyTimeTotalIncome { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal CurrencyTimeTotalSpend { get; set; }

        public bool IsCardUpdatedToLatestChanges { get; set; }
        public int Mode { get; set; }
        public string ModeName { get; set; }
        public int State { get; set; }
        public string StateName { get; set; }
        public int Type { get; set; }
        public string TypeName { get; set; }

        public virtual ICollection<Currency> Currencies { get; private set; }

        public virtual Owner Owner { get; set; }
        public virtual Users.User User { get; set; }
        public virtual CardMode CardMode { get; set; }
        public virtual CardState CardState { get; set; }


        #region public instance methods

        public Currency GetPrimaryCurrency()
        {
            return Currencies.FirstOrDefault(x => x.IsPrimary);
        }
        public Currency GetTicketCurrency()
        {
            return Currencies.FirstOrDefault(x => x.Type == CCurrency.TicketBonus && !x.IsDeleted && x.IsActive);
        }
        public Currency GetCredit2Currency()
        {
            return Currencies.FirstOrDefault(x => x.Type == CCurrency.Courtesy && !x.IsDeleted && x.IsActive);
        }

        public decimal GetBalanceFromSomeCurrency()
        {
            Currency credit = GetCredit2Currency();
            if (credit != null)
            {
                return credit.GetTotalBalance();
            }

            credit = GetTicketCurrency();
            if (credit != null)
            {
                return credit.GetTotalBalance();
            }

            credit = GetPrimaryCurrency();
            if (credit != null)
            {
                return credit.GetTotalBalance();
            }

            credit = Currencies.FirstOrDefault();
            if (credit != null)
            {
                return credit.GetTotalBalance();
            }

            return 0;
        }

        #endregion
    }
}
