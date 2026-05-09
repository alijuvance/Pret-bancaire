using PretBancaire.Data;
using PretBancaire.Models;

namespace PretBancaire.Services
{
    /// <summary>
    /// Service métier pour la gestion des clients.
    /// </summary>
    public class ClientService
    {
        private readonly ClientRepository _repo = new();

        public List<Client> GetTousClients() => _repo.GetAll();
        public Client? GetClient(int id) => _repo.GetById(id);
        public List<Client> RechercherClients(string terme) => _repo.Rechercher(terme);
        public int AjouterClient(Client c) => _repo.Ajouter(c);
        public bool ModifierClient(Client c) => _repo.Modifier(c);

        public (bool success, string message) SupprimerClient(int id)
        {
            bool result = _repo.Supprimer(id);
            if (result)
                return (true, "Client supprimé avec succès.");
            else
                return (false, "Impossible de supprimer ce client : il possède des prêts actifs.");
        }

        public bool CinExiste(string cin, int? excludeId = null) =>
            _repo.CinExiste(cin, excludeId);

        public int NombreClients() => _repo.Count();
    }
}
