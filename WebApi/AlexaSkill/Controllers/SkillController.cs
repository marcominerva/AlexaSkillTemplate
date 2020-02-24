using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AlexaSkill.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SkillController : ControllerBase
    {
        private readonly ILogger<SkillController> logger;

        public SkillController(ILogger<SkillController> logger)
        {
            this.logger = logger;
        }
    }
}
