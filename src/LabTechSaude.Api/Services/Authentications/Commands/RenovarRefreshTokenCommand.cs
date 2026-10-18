using LabTechSaude.Application.Commands.Messages;

namespace LabTechSaude.Api.Services.Authentications.Commands
{
    public class RenovarRefreshTokenCommand : ICommand
    {
        public string RefreshToken { get; }

        public RenovarRefreshTokenCommand(string refreshToken)
        {
            RefreshToken = refreshToken;
        }
    }
}
