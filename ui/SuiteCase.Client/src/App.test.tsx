import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import App from './App'

vi.mock('./features/customers/customers-page', () => ({
  CustomersPage: () => <div>Customer directory</div>,
}))

beforeEach(() => {
  window.localStorage.clear()
  delete document.documentElement.dataset.theme

  Object.defineProperty(window, 'matchMedia', {
    configurable: true,
    value: vi.fn().mockImplementation((query: string) => ({
      matches: false,
      media: query,
      onchange: null,
      addListener: vi.fn(),
      removeListener: vi.fn(),
      addEventListener: vi.fn(),
      removeEventListener: vi.fn(),
      dispatchEvent: vi.fn(),
    })),
  })
})

afterEach(() => {
  window.localStorage.clear()
  delete document.documentElement.dataset.theme
})

describe('App theme', () => {
  it('uses light mode by default and saves a theme change', async () => {
    const user = userEvent.setup()
    render(<App />)

    expect(document.documentElement).toHaveAttribute('data-theme', 'light')

    await user.click(screen.getByRole('button', { name: 'Switch to dark theme' }))

    expect(document.documentElement).toHaveAttribute('data-theme', 'dark')
    expect(window.localStorage.getItem('suitecase-theme')).toBe('dark')
    expect(screen.getByRole('button', { name: 'Switch to light theme' })).toBeVisible()
  })

  it('restores a saved dark theme', () => {
    window.localStorage.setItem('suitecase-theme', 'dark')

    render(<App />)

    expect(document.documentElement).toHaveAttribute('data-theme', 'dark')
    expect(screen.getByRole('button', { name: 'Switch to light theme' })).toBeVisible()
  })

  it('replaces an invalid saved theme with the light default', () => {
    window.localStorage.setItem('suitecase-theme', 'unknown')

    render(<App />)

    expect(document.documentElement).toHaveAttribute('data-theme', 'light')
    expect(window.localStorage.getItem('suitecase-theme')).toBe('light')
  })
})
