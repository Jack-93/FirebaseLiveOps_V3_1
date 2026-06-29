public static class SaveStatusFormatter
{
    public static string FormatShort(FirestoreManager firestore)
    {
        bool schedulerPending =
            PlayerDataSaveScheduler.Instance != null &&
            PlayerDataSaveScheduler.Instance.HasPendingRemoteSave;

        if (firestore == null)
        {
            return LocalizationManager.Text(
                "Local save only",
                "\uB85C\uCEEC \uC800\uC7A5\uB9CC");
        }

        return firestore.HasPendingSave || schedulerPending
            ? LocalizationManager.Text(
                "Local saved | Server pending",
                "\uB85C\uCEEC \uC800\uC7A5\uB428 | \uC11C\uBC84 \uB300\uAE30")
            : LocalizationManager.Text(
                "Server save complete",
                "\uC11C\uBC84 \uC800\uC7A5 \uC644\uB8CC");
    }

    public static string FormatDetailed(FirestoreManager firestore)
    {
        bool schedulerPending =
            PlayerDataSaveScheduler.Instance != null &&
            PlayerDataSaveScheduler.Instance.HasPendingRemoteSave;

        if (firestore == null)
        {
            return LocalizationManager.Text(
                "Save: local cache ready. Server unavailable.",
                "\uC800\uC7A5: \uB85C\uCEEC \uCE90\uC2DC \uC0AC\uC6A9 \uAC00\uB2A5. \uC11C\uBC84 \uC5F0\uACB0 \uC5C6\uC74C.");
        }

        if (!firestore.HasPendingSave && !schedulerPending)
        {
            return LocalizationManager.Text(
                "Save: server save complete.",
                "\uC800\uC7A5: \uC11C\uBC84 \uC800\uC7A5 \uC644\uB8CC.");
        }

        string message = LocalizationManager.Text(
            "Save: local saved. Server sync pending.",
            "\uC800\uC7A5: \uB85C\uCEEC \uC800\uC7A5 \uC644\uB8CC. \uC11C\uBC84 \uB3D9\uAE30\uD654 \uB300\uAE30.");

        if (!string.IsNullOrWhiteSpace(firestore.LastSaveError))
        {
            message += "\n" +
                LocalizationManager.Text(
                    "Last error: ",
                    "\uCD5C\uADFC \uC624\uB958: ") +
                firestore.LastSaveError;
        }

        return message;
    }

    public static string FormatManualSaveToast(FirestoreManager firestore)
    {
        bool schedulerPending =
            PlayerDataSaveScheduler.Instance != null &&
            PlayerDataSaveScheduler.Instance.HasPendingRemoteSave;

        if (firestore == null || firestore.HasPendingSave || schedulerPending)
        {
            return LocalizationManager.Text(
                "Saved locally. Server sync will retry.",
                "\uB85C\uCEEC \uC800\uC7A5 \uC644\uB8CC. \uC11C\uBC84 \uB3D9\uAE30\uD654\uB294 \uC790\uB3D9 \uC7AC\uC2DC\uB3C4.");
        }

        return LocalizationManager.Text(
            "Server save complete.",
            "\uC11C\uBC84 \uC800\uC7A5 \uC644\uB8CC.");
    }

    public static string FormatDeferredSaveToast()
    {
        return LocalizationManager.Text(
            "Saved locally. Server sync will retry.",
            "\uB85C\uCEEC \uC800\uC7A5 \uC644\uB8CC. \uC11C\uBC84 \uB3D9\uAE30\uD654\uB294 \uC790\uB3D9 \uC7AC\uC2DC\uB3C4.");
    }
}
