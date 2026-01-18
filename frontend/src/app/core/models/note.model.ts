export interface Note {
  id: number;
  title: string;
  content: string;
  color: string;
  isPinned: boolean;
  isArchived: boolean;
  isDeleted?: boolean;
  createdAt: Date;
  updatedAt?: Date;
  labels?: LabelDto[];
}

export interface LabelDto {
  id: number;
  name: string;
}

export interface CreateNoteDto {
  title?: string;
  content?: string;
  color?: string;
}

export interface UpdateNoteDto {
  title?: string;
  content?: string;
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
