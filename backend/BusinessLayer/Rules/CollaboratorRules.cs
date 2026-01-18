using BusinessLayer.Exceptions;
using ModelLayer.Enums;

namespace BusinessLayer.Rules
{
    public static class CollaboratorRules
    {
        public static void ValidatePermission(PermissionLevel permission)
        {
            if (!Enum.IsDefined(typeof(PermissionLevel), permission))
                throw new ValidationException("Invalid permission level");
        }

        public static void ValidateNoteId(int noteId)
        {
            if (noteId <= 0)
                throw new ValidationException("Invalid note ID");
        }

        public static void ValidateUserId(int userId)
        {
            if (userId <= 0)
                throw new ValidationException("Invalid user ID");
        }

        public static void ValidateCollaboratorId(int collaboratorId)
        {
            if (collaboratorId <= 0)
                throw new ValidationException("Invalid collaborator ID");
        }

        public static void ValidateAddCollaborator(int noteId, int userId, PermissionLevel permission)
        {
            ValidateNoteId(noteId);
            ValidateUserId(userId);
            ValidatePermission(permission);
        }
    }
}