using PretBancaire.Data;
using PretBancaire.Models;
using PretBancaire.Utils;

namespace PretBancaire.Services
{
    /// <summary>
    /// Service d'authentification et de gestion des utilisateurs.
    /// </summary>
    public class AuthService
    {
        private readonly UtilisateurRepository _repo = new();

        // Utilisateur actuellement connecté (null si non connecté)
        public static Utilisateur? UtilisateurConnecte { get; private set; }

        public Utilisateur? SeConnecter(string login, string motDePasse)
        {
            string hash = PasswordHelper.Hacher(motDePasse);
            var user = _repo.Authentifier(login, hash);
            UtilisateurConnecte = user;
            return user;
        }

        public void SeDeconnecter() => UtilisateurConnecte = null;

        public List<Utilisateur> GetTousUtilisateurs() => _repo.GetAll();

        public int AjouterUtilisateur(Utilisateur u)
        {
            u.MotDePasse = PasswordHelper.Hacher(u.MotDePasse);
            return _repo.Ajouter(u);
        }

        public bool ModifierUtilisateur(Utilisateur u) => _repo.Modifier(u);

        public bool ModifierMotDePasse(int id, string nouveauMdp) =>
            _repo.ModifierMotDePasse(id, PasswordHelper.Hacher(nouveauMdp));

        public bool SupprimerUtilisateur(int id) => _repo.Supprimer(id);

        public bool LoginExiste(string login, int? excludeId = null) =>
            _repo.LoginExiste(login, excludeId);
    }
}

