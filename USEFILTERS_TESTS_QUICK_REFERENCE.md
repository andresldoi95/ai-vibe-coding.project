# useFilters Tests - Quick Reference Guide

## 📁 Location
`/frontend/tests/composables/useFilters.test.ts`

## 📊 Test Stats
- **Total Tests**: 28
- **Status**: ✅ All Passing
- **Coverage**: Comprehensive

## 🧪 Test Categories

### 1️⃣ Initialization (4 tests)
Tests filter setup and initial state computation

### 2️⃣ setFilter (2 tests)
Tests updating individual filter values

### 3️⃣ resetFilters (2 tests)
Tests resetting to initial state

### 4️⃣ applyFilters (2 tests)
Tests manual filter application

### 5️⃣ activeFilterCount (4 tests)
Tests computed property for counting active filters

### 6️⃣ hasActiveFilters (3 tests)
Tests boolean computed property

### 7️⃣ onChange Callback (2 tests)
Tests callback function execution

### 8️⃣ Debouncing (3 tests)
Tests debounced updates with fake timers

### 9️⃣ Reactive Watch (3 tests)
Tests immediate reactive updates

🔟 Complex Scenarios (3 tests)
Tests real-world usage patterns

## 🔑 Key Patterns

### Testing Reactive State
```typescript
const { filters, setFilter } = useFilters({ initialFilters })
setFilter('name', 'value')
expect(filters.name).toBe('value')
```

### Testing Computed Properties
```typescript
const { activeFilterCount } = useFilters({ initialFilters })
expect(activeFilterCount.value).toBe(expectedCount)
```

### Testing with Fake Timers
```typescript
vi.useFakeTimers()
const { setFilter } = useFilters({ initialFilters, onChange, debounceMs: 300 })
setFilter('name', 'test')
vi.advanceTimersByTime(300)
expect(onChange).toHaveBeenCalled()
```

### Testing with nextTick
```typescript
setFilter('name', 'test')
await nextTick()
expect(onChange).toHaveBeenCalled()
```

## 🐛 Bug Fixed
Added missing Vue imports to composable:
```typescript
import { computed, reactive, ref, watch } from 'vue'
```

## ✅ Run Tests
```bash
npm test -- tests/composables/useFilters.test.ts
```

## 📈 Result
```
✓ tests/composables/useFilters.test.ts (28 tests) 22ms
Test Files  1 passed (1)
     Tests  28 passed (28)
```
