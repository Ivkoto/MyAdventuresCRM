using Microsoft.EntityFrameworkCore;
using SuiteCase.Core.Entities;
using CoreProgram = SuiteCase.Core.Entities.Program;

namespace SuiteCase.Server.Data;

public sealed class SuiteCaseDbContext(DbContextOptions<SuiteCaseDbContext> options) : DbContext(options)
{
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<CoreProgram> Programs => Set<CoreProgram>();
    public DbSet<ProgramOption> ProgramOptions => Set<ProgramOption>();
    public DbSet<Group> Groups => Set<Group>();
    public DbSet<GroupOption> GroupOptions => Set<GroupOption>();
    public DbSet<GroupOptionalActivity> GroupOptionalActivities => Set<GroupOptionalActivity>();
    public DbSet<ProgramPricingRule> ProgramPricingRules => Set<ProgramPricingRule>();
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
