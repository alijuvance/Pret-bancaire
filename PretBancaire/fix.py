import os

replacements = {
    'TÃ©lÃ©phone': 'Téléphone',
    'dÃ©jÃ ': 'déjà',
    'enregistrÃ©': 'enregistré',
    'succÃ¨s': 'succès',
    'SuccÃ¨s': 'Succès',
    'donnÃ©es': 'données',
    'accÃ¨s': 'accès',
    'crÃ©Ã©': 'créé',
    'CrÃ©er': 'Créer',
    'PrÃªts': 'Prêts',
    'prÃªt': 'prêt',
    'PRÃŠTS': 'PRÊTS',
    'SÃ‰LECTIONNEZ': 'SÉLECTIONNEZ',
    'SÃ©lectionnez': 'Sélectionnez',
    'Ã ': 'à',
    'MensualitÃ©': 'Mensualité',
    'ACCORDÃ‰S': 'ACCORDÉS',
    'REMBOURSÃ‰': 'REMBOURSÉ',
    'REJETÃ‰S': 'REJETÉS',
    'TERMINÃ‰S': 'TERMINÉS',
    'Ã©': 'é',
    'Ã¨': 'è',
    'Ãª': 'ê',
    'Ã¢': 'â',
    'Ã§': 'ç',
    'ðŸ’°': '💰',
    'ðŸ“‹': '📋',
    'ðŸ’³': '💳',
    'ðŸ”§': '🔧',
    'ðŸ‘¥': '👥',
    'ðŸ“ˆ': '📊',
    'ðŸšª': '🚪',
    'âž•': '➕',
    'ðŸ” ': '🔍',
    'ðŸ‘¤': '👤',
    'âœ…': '✅',
    'â–¶': '▶',
    'â Œ': '❌',
    'ðŸ’¾': '💾',
    'ðŸ—‘ï¸ ': '🗑️',
    'ðŸ”„': '🔄'
}

for root, _, files in os.walk('Forms'):
    for file in files:
        if file.endswith('.cs'):
            filepath = os.path.join(root, file)
            with open(filepath, 'r', encoding='utf-8') as f:
                content = f.read()
            original = content
            for bad, good in replacements.items():
                content = content.replace(bad, good)
            if content != original:
                with open(filepath, 'w', encoding='utf-8-sig') as f:
                    f.write(content)
                print(f'Fixed {file}')
