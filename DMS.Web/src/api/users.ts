import { request } from './http'

export interface UserDto {
  id: string
  username: string | null
  email: string | null
  createdAt: string
}

export function listUsers() {
  return request<UserDto[]>('/User')
}

export function getMe(signal?: AbortSignal) {
  return request<UserDto>('/User/me', { signal })
}

export interface LoginResponse {
  token: string
  expiresAt: string
  user: UserDto
}

export function login(usernameOrEmail: string, password: string) {
  return request<LoginResponse>('/User/login', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ usernameOrEmail, password }),
  })
}

export function register(username: string, email: string, password: string) {
  return request<UserDto>('/User/register', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ username, email, password }),
  })
}
