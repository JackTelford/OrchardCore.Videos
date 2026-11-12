"""The slides of the "Secrets: Move Existing Credentials and Change Stores" video.

assemble.py calls slide() for each step of a "slide" clip. Everything visual comes from brand.py, so the slides look
like those of every other Orchard Core video. This is an admin video: the slides hold names of the admin screens only.
"""
# brand.py is in the skill's scripts folder; assemble.py puts it on the path.
from brand import bullets, slide_base

KEPT = [
    ("1", "In the settings", "Encrypted, and kept with the settings of the feature that uses it."),
    ("2", "As a secret", "Listed on the Secrets page, and kept in a store."),
    ("check", "Migrate Secrets", "Turns the credentials kept in settings into secrets."),
]

STORES = [
    ("1", "Database", "Always available with the Secrets feature."),
    ("2", "AzureKeyVault", "Added by Secrets - Azure Key Vault, once a vault is connected."),
    ("check", "Secret Stores", "Moves secrets from one store to another."),
]

REASONS = [
    ("1", "The name is missing", "Enter the name of the secret."),
    ("2", "The name is in use", "A secret named 'Smtp.Password' already exists. Enter another name, or select the existing secret in the settings."),
    ("3", "The value does not decrypt", "The stored value could not be decrypted. Enter the value again in the settings."),
    ("4", "The store does not save it", "The secret store could not save the secret."),
]

CARE = [
    ("1", "A bulk move is not atomic", "Review every result before you retry."),
    ("2", "Existing names are kept", "A name that exists in the destination is rejected, not overwritten."),
    ("3", "The total must be zero", "Check the active secrets of an optional store, such as AzureKeyVault, before you disable it."),
    ("4", "Earlier copies remain", "Backups and vault retention are not erased by a move."),
]


def slide(step_id, chapter_title, label):
    if step_id == "tools-1":
        image = slide_base(chapter_title, label, "Where a credential can be kept")
        bullets(image, KEPT)
        return image

    if step_id == "tools-2":
        image = slide_base(chapter_title, label, "Where a secret is kept")
        bullets(image, STORES)
        return image

    if step_id == "reasons-1":
        image = slide_base(chapter_title, label, "When a credential is not moved")
        bullets(image, REASONS)
        return image

    if step_id == "care-1":
        image = slide_base(chapter_title, label, "Before you retire a store")
        bullets(image, CARE, active={0, 1})
        return image

    if step_id == "care-2":
        image = slide_base(chapter_title, label, "Before you retire a store")
        bullets(image, CARE, active={2, 3})
        return image

    raise SystemExit(f"No slide for {step_id}")
