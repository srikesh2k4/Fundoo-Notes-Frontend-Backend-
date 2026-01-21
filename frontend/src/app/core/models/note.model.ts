export interface Note {
  id: number;
  userId: number;  // ✅ ADD: Missing from your model
  title: string | null;  // ✅ FIX: Should be nullable
  content: string | null;  // ✅ FIX: Should be nullable
  color: string;
  isPinned: boolean;
  isArchived: boolean;
  isDeleted: boolean;  // ✅ FIX: Remove optional
  createdAt: Date;
  updatedAt?: Date;
  deletedAt?: Date;  // ✅ ADD: Missing
  labels: LabelDto[];  // ✅ FIX: Remove optional, use empty array default
}

export interface LabelDto {
  id: number;
  name: string;
}

export interface CreateNoteDto {
  title?: string;
  content?: string;
  color?: string;
  labelIds?: number[];  // ✅ ADD: Support label IDs on creation
}

export interface UpdateNoteDto {
  title?: string;
  content?: string;
  color?: string;
  isPinned?: boolean;
  isArchived?: boolean;
  isDeleted?: boolean;
  labelIds?: number[];  // ✅ ADD: Support updating labels
}

export interface UpdateNoteColorDto {
  color: string;
}

export interface SearchNotesDto {
  query: string;
}

export interface BulkDeleteDto {
  noteIds: number[];
}
