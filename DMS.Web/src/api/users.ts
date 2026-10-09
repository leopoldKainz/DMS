import { request } from './http'

export interface UserDto {
  id: number
  username: string | null
  email: string | null
  createdAt: string
}

export function listUsers() {
  return request<UserDto[]>('/User')
}
