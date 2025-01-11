using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Signalizer.Models;
using Signalizer.Entities.Strategies.Options;
using Signalizer.Entities.Enums;

namespace Signalizer.Context
{
    public partial class SeedData
    {
        private static readonly IEnumerable<SeedUser> seedUsers =
        [
            new SeedUser()
        {
            Email = "zy@signalizer.com",
            NormalizedEmail = "zy@signalizer.com",
            NormalizedUserName = "zy@signalizer.com",
            RoleList = [ "Administrator", "Manager" ],
            UserName = "zy@signalizer.com"
        },
        new SeedUser()
        {
            Email = "zey@signalizer.com",
            NormalizedEmail = "zey@signalizer.com",
            NormalizedUserName = "zey@signalizer.com",
            RoleList = [ "User" ],
            UserName = "zey@signalizer.com"
        },
        ];

        public static readonly IEnumerable<SignalStrategy> signalStrategies =
        [
            new SignalStrategy()
        {
            StrategyType = 0,
            Interval = 60,
            CreatedBy = "System",
            CreateDate = DateTime.Now,
            TradingPairId = 1,
            Properties = Newtonsoft.Json.JsonConvert.SerializeObject(new MaCrossoverStrategyOptions(){
                KLineInterval = Entities.Enums.KLineIntervals.OneMinute, LongPeriod=20, ShortPeriod =10}),
            IsPredefined = true,
        },
            new SignalStrategy()
        {
            StrategyType = 1,
            Interval = 60,
            CreatedBy = "System",
            CreateDate = DateTime.Now,
            TradingPairId = 2,
            Properties = Newtonsoft.Json.JsonConvert.SerializeObject(new MacdStrategyOptions(){
                KLineInterval = Entities.Enums.KLineIntervals.OneMinute, LongPeriod=12, ShortPeriod =26, Period= 9}),
            IsPredefined = true,
        },
            new SignalStrategy()
        {
            StrategyType = 2,
            Interval = 60,
            CreatedBy = "System",
            CreateDate = DateTime.Now,
            TradingPairId = 292,
            Properties = Newtonsoft.Json.JsonConvert.SerializeObject(new RsiStrategyOptions(){
                KLineInterval = Entities.Enums.KLineIntervals.OneMinute, Period=14, Overbought =70, Oversold= 30}),
            IsPredefined = true,
        },
            new SignalStrategy()
        {
            StrategyType = 3,
            Interval = 60,
            CreatedBy = "System",
            CreateDate = DateTime.Now,
            TradingPairId = 245,
            Properties = Newtonsoft.Json.JsonConvert.SerializeObject(new BollingerBandsStrategyOptions(){
                KLineInterval = Entities.Enums.KLineIntervals.OneMinute, Period=20, StandardDeviations= 2}),
            IsPredefined = true,
        },
            new SignalStrategy()
        {
            StrategyType = 4,
            Interval = 60,
            CreatedBy = "System",
            CreateDate = DateTime.Now,
            TradingPairId = 325,
            Properties = Newtonsoft.Json.JsonConvert.SerializeObject(new StochasticOscillatorStrategyOptions(){
                KLineInterval = Entities.Enums.KLineIntervals.OneMinute, Period=20, Overbought= 80, Oversold= 20}),
            IsPredefined = true,
        },
            new SignalStrategy()
        {
            StrategyType = 5,
            Interval = 60,
            CreatedBy = "System",
            CreateDate = DateTime.Now,
            TradingPairId = 334,
            Properties = Newtonsoft.Json.JsonConvert.SerializeObject(new TripleMaCrossoverStrategyOptions(){
                KLineInterval = Entities.Enums.KLineIntervals.OneMinute, ShortPeriod=5, MediumPeriod= 10, LongPeriod= 20}),
            IsPredefined = true,
        },
            new SignalStrategy()
        {
            StrategyType = 6,
            Interval = 60,
            CreatedBy = "System",
            CreateDate = DateTime.Now,
            TradingPairId = 353,
            Properties = Newtonsoft.Json.JsonConvert.SerializeObject(new PriceChannelStrategyOptions(){
                KLineInterval = Entities.Enums.KLineIntervals.OneMinute, Period= 20}),
            IsPredefined = true,
        },
            new SignalStrategy()
        {
            StrategyType = 7,
            Interval = 60,
            CreatedBy = "System",
            CreateDate = DateTime.Now,
            TradingPairId = 3,
            Properties = Newtonsoft.Json.JsonConvert.SerializeObject(new VolumePriceTrendStrategyOptions(){
                KLineInterval = Entities.Enums.KLineIntervals.OneMinute, Period= 14}),
            IsPredefined = true,
        },
            new SignalStrategy()
        {
            StrategyType = 8,
            Interval = 60,
            CreatedBy = "System",
            CreateDate = DateTime.Now,
            TradingPairId = 352,
            Properties = Newtonsoft.Json.JsonConvert.SerializeObject(new MomentumStrategyOptions(){
                KLineInterval = Entities.Enums.KLineIntervals.OneMinute, Period= 10}),
            IsPredefined = true,
        },
            new SignalStrategy()
        {
            StrategyType = 9,
            Interval = 60,
            CreatedBy = "System",
            CreateDate = DateTime.Now,
            TradingPairId = 299,
            Properties = Newtonsoft.Json.JsonConvert.SerializeObject(new ExponentialMaCrossoverWithVolumeStrategyOptions(){
                KLineInterval = Entities.Enums.KLineIntervals.OneMinute, ShortPeriod= 10, LongPeriod= 20}),
            IsPredefined = true,
        },
        ];

        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var context = new ApplicationDbContext(serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>());

            if (context.Users.Any())
            {
                return;
            }

            InsertSignalTypes(context);
            InsertStrategyTypes(context);

            var userStore = new UserStore<User>(context);
            var password = new PasswordHasher<User>();

            using var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            string[] roles = ["Administrator", "Manager", "User"];

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            using var userManager = serviceProvider.GetRequiredService<UserManager<User>>();

            foreach (var user in seedUsers)
            {
                var hashed = password.HashPassword(user, "admin");
                user.PasswordHash = hashed;
                await userStore.CreateAsync(user);

                if (user.Email is not null)
                {
                    var User = await userManager.FindByEmailAsync(user.Email);

                    if (User is not null && user.RoleList is not null)
                    {
                        await userManager.AddToRolesAsync(User, user.RoleList);
                    }
                }
            }

            foreach (var tradingPair in tradingPairs)
            {
                context.TradingPairs.Add(tradingPair);
                await context.SaveChangesAsync();
            }

            foreach (var signalStrategy in signalStrategies)
            {
                context.SignalStrategies.Add(signalStrategy);
                await context.SaveChangesAsync();
            }

            //var user2 = await userManager.FindByEmailAsync("zy@Signalizer.com");

            //Random random = new Random(tradingPairs.Count());
            //var index = random.Next();

            //foreach (var strategy in signalStrategies)
            //{
            //    context.UserSignalStrategies.Add(new UserSignalStrategy { StrategyId = strategy.Id, UserId = user2.Id });
            //}
            await context.SaveChangesAsync();
        }

        public static void InsertSignalTypes(ApplicationDbContext context)
        {
            var values = Enum.GetValues(typeof(SignalTypes));
            foreach (var value in values)
            {
                context.SignalTypes.Add(new SignalType() { Key = (int)value, Value = value.ToString() });
            }
            context.SaveChanges();
        }

        public static void InsertStrategyTypes(ApplicationDbContext context)
        {
            var values = Enum.GetValues(typeof(StrategyTypes));
            foreach (var value in values)
            {
                context.StrategyTypes.Add(new StrategyType() { Key = (int)value, Value = value.ToString() });
            }
            context.SaveChanges();
        }

        private class SeedUser : User
        {
            public string[]? RoleList { get; set; }
        }
    }
}
