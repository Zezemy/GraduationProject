using Microsoft.AspNetCore.Mvc;
using Signalizer.Context;
using Signalizer.Entities;
using Signalizer.Entities.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Signalizer.Models;

namespace Signalizer.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]

    public class CommandServiceController : ControllerBase
    {
        private ApplicationDbContext _context { get; }
        private IHttpContextAccessor _accessor { get; }

        private readonly ILogger<CommandServiceController> _logger;
        public CommandServiceController(ApplicationDbContext context, ILogger<CommandServiceController> logger, IHttpContextAccessor accessor)
        {
            _context = context;
            _logger = logger;
            _accessor = accessor;
        }


        [HttpPost(Name = "CreateStrategy")]
        public async Task<object> CreateStrategy([FromBody] CreateStrategyRequestMessage msg)
        {
            try
            {
                var userId = _accessor.HttpContext.User.Claims.ToList()[0].Value;
                var strategy = new Models.SignalStrategy()
                {
                    CreatedBy = userId,
                    Interval = msg.SignalStrategy.Interval == 0 ? (int)KLineIntervals.OneHour : msg.SignalStrategy.Interval,
                    IsPredefined = msg.SignalStrategy.IsPredefined,
                    CreateDate = DateTime.Now,
                    StrategyType = msg.SignalStrategy.StrategyType,
                    TradingPairId = msg.SignalStrategy.TradingPair.Id,
                    Properties = msg.SignalStrategy.Properties
                };
                _context.SignalStrategies.Add(strategy);
                await _context.SaveChangesAsync();
                _context.UserSignalStrategies.Add(new UserSignalStrategy { StrategyId = strategy.Id, UserId = userId });
                await _context.SaveChangesAsync();

                return new CreateStrategyResponseMessage
                {
                    ResponseCode = "0",
                    ResponseDescription = "Transaction is successful."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return new CreateStrategyResponseMessage
                {
                    ResponseCode = "1",
                    ResponseDescription = "Transaction is failed."
                };
            }
        }

        [HttpPost(Name = "UpdateStrategy")]
        public async Task<object> UpdateStrategy([FromBody] UpdateStrategyRequestMessage msg)
        {
            try
            {
                var strategy = _context.SignalStrategies.Where(x => x.Id == msg.SignalStrategy.Id).FirstOrDefault();

                if (msg.SignalStrategy.StrategyType >= 0)
                {
                    strategy.StrategyType = msg.SignalStrategy.StrategyType;
                }

                if (msg.SignalStrategy.TradingPair.Id >= 0)
                {
                    strategy.TradingPairId = msg.SignalStrategy.TradingPair.Id;
                }

                if (msg.SignalStrategy.Interval >= 0)
                {
                    strategy.Interval = msg.SignalStrategy.Interval;
                }

                strategy.UpdatedBy = _accessor.HttpContext.User.Claims.ToList()[0].Value;
                strategy.UpdateDate = DateTime.Now;
                strategy.IsPredefined = msg.SignalStrategy.IsPredefined;
                strategy.Properties = msg.SignalStrategy.Properties;

                await _context.SaveChangesAsync();
                return new UpdateStrategyResponseMessage
                {
                    ResponseCode = "0",
                    ResponseDescription = "Transaction is successful."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return new UpdateStrategyResponseMessage
                {
                    ResponseCode = "1",
                    ResponseDescription = "Transaction is failed."
                };
            }
        }

        [HttpPost(Name = "AddUser")]
        public async Task<object> AddUserAsync(AddUserRequestMessage req)
        {
            var user = new User();

            try
            {
                if (!string.IsNullOrWhiteSpace(req.UserModel.Username))
                {
                    user.UserName = req.UserModel.Username;
                    user.NormalizedUserName = req.UserModel.Username.ToUpper();
                }
                else
                {
                    return new BaseResponse
                    {
                        ResponseCode = "2",
                        ResponseDescription = "User Name cannot be empty."
                    };
                }

                if (!string.IsNullOrWhiteSpace(req.UserModel.Email))
                {
                    user.Email = req.UserModel.Email;
                    user.NormalizedEmail = req.UserModel.Email.ToUpper();
                }
                else
                {
                    return new BaseResponse
                    {
                        ResponseCode = "3",
                        ResponseDescription = "Email cannot be empty."
                    };
                }

                if (!string.IsNullOrWhiteSpace(req.UserModel.Password))
                {
                    var passwordHasher = new PasswordHasher<User>();
                    var hashed = passwordHasher.HashPassword(user, $"{req.UserModel.Password}");
                    user.PasswordHash = hashed;
                }
                else
                {
                    return new BaseResponse
                    {
                        ResponseCode = "4",
                        ResponseDescription = "Password cannot be empty."
                    };
                }

                var userStore = new UserStore<User>(_context);
                await userStore.CreateAsync(user);
                await _context.SaveChangesAsync();
                return new BaseResponse
                {
                    ResponseCode = "0",
                    ResponseDescription = "Transaction is successful."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return new BaseResponse
                {
                    ResponseCode = "1",
                    ResponseDescription = "Transaction is failed."
                };
            }
        }

        [HttpPost(Name = "UpdateUser")]
        public async Task<object> UpdateUserAsync(UpdateUserRequestMessage req)
        {
            try
            {
                var user = _context.Users.Where(x => x.Id == req.UserModel.Id).FirstOrDefault();

                if (!string.IsNullOrWhiteSpace(req.UserModel.Username))
                {
                    user.UserName = req.UserModel.Username;
                }

                if (!string.IsNullOrWhiteSpace(req.UserModel.Email))
                {
                    user.Email = req.UserModel.Email;
                }

                if (!string.IsNullOrWhiteSpace(req.UserModel.Password))
                {
                    var passwordHasher = new PasswordHasher<User>();
                    var hashed = passwordHasher.HashPassword(user, $"{req.UserModel.Password}");
                    user.PasswordHash = hashed;
                }

                _context.SaveChanges();
                return new BaseResponse
                {
                    ResponseCode = "0",
                    ResponseDescription = "Transaction is successful."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return new BaseResponse
                {
                    ResponseCode = "1",
                    ResponseDescription = "Transaction is failed."
                };
            }
        }

        [HttpDelete(Name = "DeleteUserById")]
        public async Task<object> DeleteUserByIdAsync(string id)
        {
            try
            {
                var user = _context.Users.Where(x => x.Id == id).FirstOrDefault();
                _context.Users.Remove(user);
                _context.SaveChanges();
                return new BaseResponse
                {
                    ResponseCode = "0",
                    ResponseDescription = "Transaction is successful."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return new BaseResponse
                {
                    ResponseCode = "1",
                    ResponseDescription = "Transaction is failed."
                };
            }
        }

        [HttpDelete(Name = "DeleteStrategyById")]
        public async Task<object> DeleteStrategyByIdAsync(long id)
        {
            try
            {
                var strategy = _context.SignalStrategies.Where(x => x.Id == id).FirstOrDefault();
                _context.SignalStrategies.Remove(strategy);
                _context.SaveChanges();
                return new BaseResponse
                {
                    ResponseCode = "0",
                    ResponseDescription = "Transaction is successful."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return new BaseResponse
                {
                    ResponseCode = "1",
                    ResponseDescription = "Transaction is failed."
                };
            }
        }
    }
}
