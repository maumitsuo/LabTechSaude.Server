namespace LabTechSaude.Api.Notifications
{
    public class Notificador : IDisposable
    {
        private readonly List<string> _notificacoes;
        public IReadOnlyCollection<string> Notificacoes => _notificacoes.AsReadOnly();

        public Notificador()
        {
            _notificacoes = new List<string>();
        }

        public void IncluirNotificacao(string mensagem)
        {
            _notificacoes.Add(mensagem);
        }
        
        public bool ExistemNotificacoes() => _notificacoes.Any();
        
        public void LimparNotificacoes() => _notificacoes.Clear();

        public void Dispose()
        {
            LimparNotificacoes();
        }
    }
}
