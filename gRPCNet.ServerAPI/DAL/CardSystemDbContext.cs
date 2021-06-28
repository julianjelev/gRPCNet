using gRPCNet.ServerAPI.Models.Domain.Cards;
using gRPCNet.ServerAPI.Models.Domain.CashRegisters;
using gRPCNet.ServerAPI.Models.Domain.Common;
using gRPCNet.ServerAPI.Models.Domain.EGMs;
using gRPCNet.ServerAPI.Models.Domain.Owners;
using gRPCNet.ServerAPI.Models.Domain.Places;
using gRPCNet.ServerAPI.Models.Domain.Users;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace gRPCNet.ServerAPI.DAL
{
    public class CardSystemDbContext : IdentityDbContext<User>
    {
        public CardSystemDbContext(DbContextOptions<CardSystemDbContext> options) 
            : base(options)
        {
            Database.SetCommandTimeout(300);
        }

        #region Cards
        public DbSet<Card> Cards { get; set; }
        public DbSet<CardType> CardTypes { get; set; }
        public DbSet<CardState> CardStates { get; set; }
        public DbSet<CardMode> CardModes { get; set; }
        public DbSet<CardProfile> CardProfiles { get; set; }
        #endregion

        #region CashRegisters
        public DbSet<Article> Articles { get; set; }
        public DbSet<ArticleCategory> ArticleCategories { get; set; }
        public DbSet<BonusPack> BonusPacks { get; set; }
        public DbSet<PlacesArticles> PlacesArticles { get; set; }
        #endregion

        #region Common
        public DbSet<AllowedCurrency> AllowedCurrencies { get; set; }
        public DbSet<Currency> Currencies { get; set; }
        public DbSet<CurrencyTemplate> CurrencyTemplates { get; set; }
        public DbSet<ImageInfo> ImageInfos { get; set; }
        public DbSet<KVP> KVPs { get; set; }
        public DbSet<ScheduleDay> ScheduleDays { get; set; }
        public DbSet<ScheduleHour> ScheduleHours { get; set; }
        #endregion

        #region EGMs
        public DbSet<ControllerConfigRelay> ControllerConfigRelays { get; set; }
        public DbSet<EGM> EGMs { get; set; }
        public DbSet<GameCategory> GameCategories { get; set; }
        public DbSet<GameManageController> GameManageControllers { get; set; }
        public DbSet<LightingDiode> LightingDiodes { get; set; }
        public DbSet<LightingEvent> LightingEvents { get; set; }
        public DbSet<LightingProfile> LightingProfiles { get; set; }
        public DbSet<LightingSignal> LightingSignals { get; set; }
        #endregion

        #region Owners
        public DbSet<Owner> Owners { get; set; }
        public DbSet<UserOwner> UserOwners { get; set; }
        #endregion

        #region Places
        public DbSet<Concentrator> Concentrators { get; set; }
        public DbSet<GameCenter> GameCenters { get; set; }
        #endregion

        protected override void OnModelCreating(ModelBuilder builder) 
        {
            #region Cards
            builder.Entity<Card>()
                .HasOne(x => x.Owner)
                .WithMany(x => x.Cards)
                .HasForeignKey(x => x.OwnerId)
                .IsRequired();
            builder.Entity<Card>()
                .HasOne(x => x.User)
                .WithMany(x => x.Cards)
                .HasForeignKey(x => x.UserId)
                .IsRequired(false);
            builder.Entity<Card>()
                .HasOne(x => x.CardMode)
                .WithMany(x => x.Cards)
                .HasForeignKey(x => x.CardModeId)
                .IsRequired(false);
            builder.Entity<Card>()
                .HasOne(x => x.CardState)
                .WithMany(x => x.Cards)
                .HasForeignKey(x => x.CardStateId)
                .IsRequired(false);
            #endregion

            #region CashRegisters
            builder.Entity<PlacesArticles>()
                .HasKey(x => new { x.GameCenterId, x.ArticleId });
            builder.Entity<PlacesArticles>()
                .HasOne(x => x.GameCenter)
                .WithMany(x => x.Articles)
                .HasForeignKey(x => x.GameCenterId);
            builder.Entity<PlacesArticles>()
                .HasOne(x => x.Article)
                .WithMany(x => x.Places)
                .HasForeignKey(x => x.ArticleId);
            #endregion

            #region Owners
            builder.Entity<UserOwner>()
                .HasKey(x => new { x.UserId, x.OwnerId });
            builder.Entity<UserOwner>()
                .HasOne(x => x.Owner)
                .WithMany(x => x.Users)
                .HasForeignKey(x => x.OwnerId);
            builder.Entity<UserOwner>()
                .HasOne(x => x.User)
                .WithMany(x => x.Owners)
                .HasForeignKey(x => x.UserId);
            #endregion


            builder.Entity<UserPlace>()
                .HasKey(x => new { x.UserId, x.GameCenterId });
            builder.Entity<UserPlace>()
                .HasOne(x => x.GameCenter)
                .WithMany(x => x.Workers)
                .HasForeignKey(x => x.GameCenterId);
            builder.Entity<UserPlace>()
                .HasOne(x => x.User)
                .WithMany(x => x.Places)
                .HasForeignKey(x => x.UserId);

            base.OnModelCreating(builder);
        }
    }
}
