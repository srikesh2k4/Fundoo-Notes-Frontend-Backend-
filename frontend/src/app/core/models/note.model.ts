export interface Note {
  id: number;
  userId: number;  
  title: string | null;  
  content: string | null; 
  color: string;
  isPinned: boolean;
  isArchived: boolean;
  isDeleted: boolean;  
  createdAt: Date;
  updatedAt?: Date;
  deletedAt?: Date;  
  labels: LabelDto[];  
}

export interface LabelDto {
  id: number;
  name: string;
}

export interface CreateNoteDto {
  title?: string;
  content?: string;
  color?: string;
  labelIds?: number[]; 
}

export interface UpdateNoteDto {
  title?: string;
  content?: string;
  color?: string;
  isPinned?: boolean;
  isArchived?: boolean;
  isDeleted?: boolean;
  labelIds?: number[]; 
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
