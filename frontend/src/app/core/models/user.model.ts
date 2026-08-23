import { UserRole } from './enums';

export interface User {
  id: number;
  fullName: string;
  email: string;
  role: UserRole;
  createdAt?: string;
}
