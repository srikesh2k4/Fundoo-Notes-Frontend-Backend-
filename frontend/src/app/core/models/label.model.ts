export interface Label {
  id: number;
  name: string;
  createdAt: Date;
}

export interface CreateLabelDto {
  name: string;
}

export interface UpdateLabelDto {
  name: string;
}
