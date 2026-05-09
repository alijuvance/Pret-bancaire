-- =============================================================================
-- Système de Prêt Bancaire — Script de création de la base de données
-- SGBD : MySQL 8.x
-- Date : 2026-05-09
-- =============================================================================

-- Création de la base de données
CREATE DATABASE IF NOT EXISTS pret_bancaire
    CHARACTER SET utf8mb4
    COLLATE utf8mb4_unicode_ci;

USE pret_bancaire;

-- =============================================================================
-- TABLE 1 : UTILISATEURS (Comptes de connexion au système)
-- =============================================================================
CREATE TABLE IF NOT EXISTS utilisateurs (
    id              INT             AUTO_INCREMENT PRIMARY KEY,
    nom             VARCHAR(100)    NOT NULL,
    prenom          VARCHAR(100)    NOT NULL,
    login           VARCHAR(50)     NOT NULL UNIQUE,
    mot_de_passe    VARCHAR(255)    NOT NULL,
    role            ENUM('Admin', 'Agent') NOT NULL DEFAULT 'Agent',
    date_creation   DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
    actif           BOOLEAN         NOT NULL DEFAULT TRUE,

    INDEX idx_login (login),
    INDEX idx_role (role)
) ENGINE=InnoDB;

-- =============================================================================
-- TABLE 2 : CLIENTS (Personnes demandant un prêt)
-- =============================================================================
CREATE TABLE IF NOT EXISTS clients (
    id                  INT             AUTO_INCREMENT PRIMARY KEY,
    nom                 VARCHAR(100)    NOT NULL,
    prenom              VARCHAR(100)    NOT NULL,
    date_naissance      DATE            NOT NULL,
    cin                 VARCHAR(20)     NOT NULL UNIQUE,
    telephone           VARCHAR(20)     NOT NULL,
    adresse             VARCHAR(255)    DEFAULT NULL,
    email               VARCHAR(100)    DEFAULT NULL,
    date_inscription    DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
    actif               BOOLEAN         NOT NULL DEFAULT TRUE,

    INDEX idx_cin (cin),
    INDEX idx_nom_prenom (nom, prenom)
) ENGINE=InnoDB;

-- =============================================================================
-- TABLE 3 : PRETS (Demandes de prêts associées aux clients)
-- =============================================================================
CREATE TABLE IF NOT EXISTS prets (
    id                  INT             AUTO_INCREMENT PRIMARY KEY,
    client_id           INT             NOT NULL,
    montant             DECIMAL(15,2)   NOT NULL,
    taux_interet        DECIMAL(5,2)    NOT NULL,
    duree_mois          INT             NOT NULL,
    mensualite          DECIMAL(15,2)   NOT NULL,
    montant_total       DECIMAL(15,2)   NOT NULL,
    statut              ENUM('EnAttente', 'Approuve', 'EnCours', 'Termine', 'Rejete')
                                        NOT NULL DEFAULT 'EnAttente',
    date_demande        DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,
    date_approbation    DATETIME        DEFAULT NULL,
    notes               TEXT            DEFAULT NULL,

    CONSTRAINT fk_prets_client
        FOREIGN KEY (client_id) REFERENCES clients(id)
        ON UPDATE CASCADE
        ON DELETE RESTRICT,

    INDEX idx_client_id (client_id),
    INDEX idx_statut (statut),
    INDEX idx_date_demande (date_demande)
) ENGINE=InnoDB;

-- =============================================================================
-- TABLE 4 : PAIEMENTS (Remboursements effectués sur un prêt)
-- =============================================================================
CREATE TABLE IF NOT EXISTS paiements (
    id              INT             AUTO_INCREMENT PRIMARY KEY,
    pret_id         INT             NOT NULL,
    montant         DECIMAL(15,2)   NOT NULL,
    date_paiement   DATE            NOT NULL,
    mode_paiement   ENUM('Especes', 'Virement', 'Cheque', 'CarteBancaire')
                                    NOT NULL,
    reference       VARCHAR(50)     DEFAULT NULL,
    notes           TEXT            DEFAULT NULL,

    CONSTRAINT fk_paiements_pret
        FOREIGN KEY (pret_id) REFERENCES prets(id)
        ON UPDATE CASCADE
        ON DELETE RESTRICT,

    INDEX idx_pret_id (pret_id),
    INDEX idx_date_paiement (date_paiement)
) ENGINE=InnoDB;

-- =============================================================================
-- DONNÉES INITIALES : Compte administrateur par défaut
-- Mot de passe : "admin123" (haché en SHA-256 avec sel "PretBancaire_")
-- =============================================================================
INSERT INTO utilisateurs (nom, prenom, login, mot_de_passe, role)
VALUES (
    'Administrateur',
    'Système',
    'admin',
    -- SHA-256 de "PretBancaire_admin123"
    'f129e32ef6db259d056fed2c1953ae408e555e53a6ec6c78d4e5775d577a6296',
    'Admin'
);


