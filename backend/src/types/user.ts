export interface IUser {
  id: string;
  email: string;
  passwordHash: string;
  fullName: string;
  phone: string | null;
  status: string;
  lastLoginAt: Date | null;
  createdAt: Date;
  updatedAt: Date;
}

export interface IUserCompany {
  companyId: string;
  companyName: string;
  companyRuc: string;
  roleName: string;
  roleId: string;
}

export interface ICreateUser {
  email: string;
  password: string;
  fullName: string;
  phone?: string;
  companyId: string;
  roleId: string;
}

export interface IUserWithCompanies extends Omit<IUser, 'passwordHash'> {
  companies: IUserCompany[];
}
