using LabTechSaude.Application.Commands.Messages;

namespace LabTechSaude.Api.Services.Authentications.Commands
{
    public class RevogarRefreshTokenCommand : ICommand
    {
        public string RefreshToken { get; }

        public RevogarRefreshTokenCommand(string refreshToken)
        {
            RefreshToken = refreshToken;
        }
    }
}
