export type LoginResponse = {
  token: string
  expiresAt: string
  refreshToken: string
  refreshExpiresAt: string
  username: string
  displayName: string
  roles: string[]
  permissions: string[]
}

