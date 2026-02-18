export interface CompanyUser {
  id: string
  name: string
  email: string
  role: {
    id: string
    name: string
  }
  joinedAt: string
  isActive: boolean
}

export interface InviteUserData {
  email: string
  roleId: string
  personalMessage?: string
}

export interface UpdateUserRoleData {
  userId: string
  roleId: string
}

export interface AcceptInvitationData {
  invitationToken: string
  name?: string
  password?: string
}

export interface AcceptInvitationResponse {
  accessToken: string
  refreshToken: string
  user: {
    id: string
    name: string
    email: string
    isActive: boolean
    emailConfirmed: boolean
  }
  tenants: Array<{
    id: string
    name: string
    slug: string
    status: string
  }>
}
