using Microsoft.EntityFrameworkCore;
using SuiteCase.Core.Entities;

namespace SuiteCase.Server.Data;

public sealed class SuiteCaseDbContext(DbContextOptions<SuiteCaseDbContext> options) : DbContext(options)
{
    public DbSet<AuditEvent> AuditEvents => Set<AuditEvent>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<TravelProgram> TravelPrograms => Set<TravelProgram>();
    public DbSet<TravelProgramOption> TravelProgramOptions => Set<TravelProgramOption>();
    public DbSet<TravelProgramPricingRule> TravelProgramPricingRules => Set<TravelProgramPricingRule>();
    public DbSet<Group> Groups => Set<Group>();
    public DbSet<GroupOption> GroupOptions => Set<GroupOption>();
    public DbSet<GroupOptionalActivity> GroupOptionalActivities => Set<GroupOptionalActivity>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<BookingOption> BookingOptions => Set<BookingOption>();
    public DbSet<BookingOptionalActivity> BookingOptionalActivities => Set<BookingOptionalActivity>();
    public DbSet<BookingItem> BookingItems => Set<BookingItem>();
    public DbSet<BookingTravelLeg> BookingTravelLegs => Set<BookingTravelLeg>();
    public DbSet<PaymentMilestone> PaymentMilestones => Set<PaymentMilestone>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<LoyaltyDiscountRule> LoyaltyDiscountRules => Set<LoyaltyDiscountRule>();
    public DbSet<LoyaltyDiscountRuleDestination> LoyaltyDiscountRuleDestinations => Set<LoyaltyDiscountRuleDestination>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SuiteCaseDbContext).Assembly);
    }
}
