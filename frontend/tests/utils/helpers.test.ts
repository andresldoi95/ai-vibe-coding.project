import { describe, expect, it } from 'vitest'
import {
  capitalize,
  clamp,
  generateRandomString,
  groupBy,
  omit,
  pick,
  roundToDecimals,
  slugify,
  truncate,
  unique,
} from '~/utils/helpers'

describe('helpers utility', () => {
  describe('string utilities', () => {
    describe('truncate', () => {
      it('should truncate text longer than maxLength', () => {
        const result = truncate('This is a long text', 10)
        expect(result).toBe('This is a ...')
      })

      it('should not truncate text shorter than maxLength', () => {
        const result = truncate('Short', 10)
        expect(result).toBe('Short')
      })

      it('should not truncate text equal to maxLength', () => {
        const result = truncate('Exactly10!', 10)
        expect(result).toBe('Exactly10!')
      })

      it('should handle empty string', () => {
        const result = truncate('', 10)
        expect(result).toBe('')
      })

      it('should handle zero maxLength', () => {
        const result = truncate('Text', 0)
        expect(result).toBe('...')
      })
    })

    describe('capitalize', () => {
      it('should capitalize first letter of lowercase word', () => {
        const result = capitalize('hello')
        expect(result).toBe('Hello')
      })

      it('should lowercase rest of the word', () => {
        const result = capitalize('HELLO')
        expect(result).toBe('Hello')
      })

      it('should handle mixed case', () => {
        const result = capitalize('hELLo')
        expect(result).toBe('Hello')
      })

      it('should handle single character', () => {
        const result = capitalize('a')
        expect(result).toBe('A')
      })

      it('should handle empty string', () => {
        const result = capitalize('')
        expect(result).toBe('')
      })

      it('should handle multiple words', () => {
        const result = capitalize('hello world')
        expect(result).toBe('Hello world')
      })
    })

    describe('slugify', () => {
      it('should convert text to lowercase slug', () => {
        const result = slugify('Hello World')
        expect(result).toBe('hello-world')
      })

      it('should replace spaces with dashes', () => {
        const result = slugify('Product Name Example')
        expect(result).toBe('product-name-example')
      })

      it('should remove special characters', () => {
        const result = slugify('Product@Name#Example!')
        expect(result).toBe('productnameexample')
      })

      it('should replace multiple spaces with single dash', () => {
        const result = slugify('Product    Name')
        expect(result).toBe('product-name')
      })

      it('should replace underscores with dashes', () => {
        const result = slugify('product_name')
        expect(result).toBe('product-name')
      })

      it('should trim leading and trailing dashes', () => {
        const result = slugify('  product name  ')
        expect(result).toBe('product-name')
      })

      it('should handle already slugified text', () => {
        const result = slugify('already-slugified')
        expect(result).toBe('already-slugified')
      })

      it('should handle numbers', () => {
        const result = slugify('Product 123')
        expect(result).toBe('product-123')
      })
    })

    describe('generateRandomString', () => {
      it('should generate string of default length 10', () => {
        const result = generateRandomString()
        expect(result).toHaveLength(10)
      })

      it('should generate string of specified length', () => {
        const result = generateRandomString(20)
        expect(result).toHaveLength(20)
      })

      it('should generate string with only alphanumeric characters', () => {
        const result = generateRandomString(100)
        expect(result).toMatch(/^[a-z0-9]+$/i)
      })

      it('should generate different strings on consecutive calls', () => {
        const result1 = generateRandomString(10)
        const result2 = generateRandomString(10)
        expect(result1).not.toBe(result2)
      })

      it('should generate single character', () => {
        const result = generateRandomString(1)
        expect(result).toHaveLength(1)
      })

      it('should handle length of 0', () => {
        const result = generateRandomString(0)
        expect(result).toBe('')
      })
    })
  })

  describe('number utilities', () => {
    describe('clamp', () => {
      it('should clamp value below minimum', () => {
        const result = clamp(5, 10, 20)
        expect(result).toBe(10)
      })

      it('should clamp value above maximum', () => {
        const result = clamp(25, 10, 20)
        expect(result).toBe(20)
      })

      it('should return value within range', () => {
        const result = clamp(15, 10, 20)
        expect(result).toBe(15)
      })

      it('should handle value equal to minimum', () => {
        const result = clamp(10, 10, 20)
        expect(result).toBe(10)
      })

      it('should handle value equal to maximum', () => {
        const result = clamp(20, 10, 20)
        expect(result).toBe(20)
      })

      it('should handle negative numbers', () => {
        const result = clamp(-5, -10, 0)
        expect(result).toBe(-5)
      })

      it('should handle floating point numbers', () => {
        const result = clamp(15.5, 10.2, 20.8)
        expect(result).toBe(15.5)
      })
    })

    describe('roundToDecimals', () => {
      it('should round to 2 decimals by default', () => {
        const result = roundToDecimals(3.14159)
        expect(result).toBe(3.14)
      })

      it('should round to specified decimals', () => {
        const result = roundToDecimals(3.14159, 3)
        expect(result).toBe(3.142)
      })

      it('should round up correctly', () => {
        const result = roundToDecimals(3.145, 2)
        expect(result).toBe(3.15)
      })

      it('should round down correctly', () => {
        const result = roundToDecimals(3.144, 2)
        expect(result).toBe(3.14)
      })

      it('should handle zero decimals', () => {
        const result = roundToDecimals(3.7, 0)
        expect(result).toBe(4)
      })

      it('should handle integer values', () => {
        const result = roundToDecimals(5, 2)
        expect(result).toBe(5)
      })

      it('should handle negative numbers', () => {
        const result = roundToDecimals(-3.145, 2)
        expect(result).toBe(-3.14)
      })
    })
  })

  describe('array utilities', () => {
    describe('unique', () => {
      it('should remove duplicate strings', () => {
        const result = unique(['a', 'b', 'a', 'c', 'b'])
        expect(result).toEqual(['a', 'b', 'c'])
      })

      it('should remove duplicate numbers', () => {
        const result = unique([1, 2, 1, 3, 2])
        expect(result).toEqual([1, 2, 3])
      })

      it('should handle array with no duplicates', () => {
        const result = unique([1, 2, 3])
        expect(result).toEqual([1, 2, 3])
      })

      it('should handle empty array', () => {
        const result = unique([])
        expect(result).toEqual([])
      })

      it('should handle single element', () => {
        const result = unique([1])
        expect(result).toEqual([1])
      })

      it('should maintain order of first occurrence', () => {
        const result = unique([3, 1, 2, 1, 3])
        expect(result).toEqual([3, 1, 2])
      })
    })

    describe('groupBy', () => {
      it('should group objects by specified key', () => {
        const items = [
          { id: '1', category: 'A' },
          { id: '2', category: 'B' },
          { id: '3', category: 'A' },
        ]
        const result = groupBy(items, 'category')
        expect(result).toEqual({
          A: [
            { id: '1', category: 'A' },
            { id: '3', category: 'A' },
          ],
          B: [{ id: '2', category: 'B' }],
        })
      })

      it('should handle numeric keys', () => {
        const items = [
          { id: '1', count: 10 },
          { id: '2', count: 20 },
          { id: '3', count: 10 },
        ]
        const result = groupBy(items, 'count')
        expect(result).toEqual({
          10: [
            { id: '1', count: 10 },
            { id: '3', count: 10 },
          ],
          20: [{ id: '2', count: 20 }],
        })
      })

      it('should handle empty array', () => {
        const result = groupBy([], 'category')
        expect(result).toEqual({})
      })

      it('should handle single item', () => {
        const items = [{ id: '1', category: 'A' }]
        const result = groupBy(items, 'category')
        expect(result).toEqual({
          A: [{ id: '1', category: 'A' }],
        })
      })
    })
  })

  describe('object utilities', () => {
    describe('omit', () => {
      it('should omit specified keys', () => {
        const obj = { a: 1, b: 2, c: 3 }
        const result = omit(obj, ['b'])
        expect(result).toEqual({ a: 1, c: 3 })
      })

      it('should omit multiple keys', () => {
        const obj = { a: 1, b: 2, c: 3, d: 4 }
        const result = omit(obj, ['b', 'd'])
        expect(result).toEqual({ a: 1, c: 3 })
      })

      it('should handle empty keys array', () => {
        const obj = { a: 1, b: 2 }
        const result = omit(obj, [])
        expect(result).toEqual({ a: 1, b: 2 })
      })

      it('should handle non-existent keys', () => {
        const obj = { a: 1, b: 2 }
        const result = omit(obj, ['c' as keyof typeof obj])
        expect(result).toEqual({ a: 1, b: 2 })
      })

      it('should not mutate original object', () => {
        const obj = { a: 1, b: 2, c: 3 }
        omit(obj, ['b'])
        expect(obj).toEqual({ a: 1, b: 2, c: 3 })
      })
    })

    describe('pick', () => {
      it('should pick specified keys', () => {
        const obj = { a: 1, b: 2, c: 3 }
        const result = pick(obj, ['a', 'c'])
        expect(result).toEqual({ a: 1, c: 3 })
      })

      it('should pick single key', () => {
        const obj = { a: 1, b: 2, c: 3 }
        const result = pick(obj, ['b'])
        expect(result).toEqual({ b: 2 })
      })

      it('should handle empty keys array', () => {
        const obj = { a: 1, b: 2 }
        const result = pick(obj, [])
        expect(result).toEqual({})
      })

      it('should handle non-existent keys', () => {
        const obj = { a: 1, b: 2 }
        const result = pick(obj, ['c' as keyof typeof obj])
        expect(result).toEqual({})
      })

      it('should not mutate original object', () => {
        const obj = { a: 1, b: 2, c: 3 }
        pick(obj, ['a'])
        expect(obj).toEqual({ a: 1, b: 2, c: 3 })
      })

      it('should pick all keys', () => {
        const obj = { a: 1, b: 2 }
        const result = pick(obj, ['a', 'b'])
        expect(result).toEqual({ a: 1, b: 2 })
      })
    })
  })
})
