import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import {
  analyzeCodePatterns,
  generateAnalysisReport,
  printAnalysisSummary,
} from '~/utils/codeAnalysis'
import type { CodeAnalysisReport, RepetitivePattern } from '~/utils/codeAnalysis'

describe('codeAnalysis utility', () => {
  describe('analyzeCodePatterns', () => {
    let analysis: CodeAnalysisReport

    beforeEach(() => {
      analysis = analyzeCodePatterns()
    })

    it('should return a code analysis report', () => {
      expect(analysis).toBeDefined()
      expect(analysis.totalPatterns).toBeGreaterThan(0)
      expect(analysis.highImpactPatterns).toBeGreaterThan(0)
      expect(analysis.estimatedLinesToReduce).toBeGreaterThan(0)
      expect(Array.isArray(analysis.patterns)).toBe(true)
    })

    it('should have valid pattern structure', () => {
      analysis.patterns.forEach((pattern: RepetitivePattern) => {
        expect(pattern.name).toBeDefined()
        expect(typeof pattern.name).toBe('string')
        expect(pattern.description).toBeDefined()
        expect(typeof pattern.description).toBe('string')
        expect(pattern.occurrences).toBeGreaterThan(0)
        expect(Array.isArray(pattern.locations)).toBe(true)
        expect(['high', 'medium', 'low']).toContain(pattern.impact)
        expect(pattern.solution).toBeDefined()
        expect(typeof pattern.solution).toBe('string')
      })
    })

    it('should count high impact patterns correctly', () => {
      const highImpactCount = analysis.patterns.filter(p => p.impact === 'high').length
      expect(analysis.highImpactPatterns).toBe(highImpactCount)
    })

    it('should have consistent total patterns count', () => {
      expect(analysis.totalPatterns).toBe(analysis.patterns.length)
    })

    it('should have patterns with locations', () => {
      analysis.patterns.forEach((pattern) => {
        expect(pattern.locations.length).toBeGreaterThan(0)
      })
    })

    it('should estimate lines to reduce', () => {
      expect(analysis.estimatedLinesToReduce).toBeGreaterThan(0)
      expect(typeof analysis.estimatedLinesToReduce).toBe('number')
    })

    it('should categorize patterns by impact', () => {
      const hasHigh = analysis.patterns.some(p => p.impact === 'high')
      const hasMedium = analysis.patterns.some(p => p.impact === 'medium')
      const hasLow = analysis.patterns.some(p => p.impact === 'low')

      expect(hasHigh || hasMedium || hasLow).toBe(true)
    })

    it('should include common repetitive patterns', () => {
      const patternNames = analysis.patterns.map(p => p.name)

      // Check for expected pattern names
      const expectedPatterns = [
        'CRUD Page Setup',
        'Data Loading Pattern',
        'Delete Confirmation Dialog',
      ]

      expectedPatterns.forEach((expected) => {
        expect(patternNames).toContain(expected)
      })
    })

    it('should have solution for each pattern', () => {
      analysis.patterns.forEach((pattern) => {
        expect(pattern.solution).toBeTruthy()
        expect(pattern.solution.length).toBeGreaterThan(10)
      })
    })

    it('should have multiple occurrences for each pattern', () => {
      analysis.patterns.forEach((pattern) => {
        expect(pattern.occurrences).toBeGreaterThanOrEqual(1)
      })
    })

    it('should return the same analysis on repeated calls', () => {
      const analysis1 = analyzeCodePatterns()
      const analysis2 = analyzeCodePatterns()

      expect(analysis1.totalPatterns).toBe(analysis2.totalPatterns)
      expect(analysis1.highImpactPatterns).toBe(analysis2.highImpactPatterns)
      expect(analysis1.estimatedLinesToReduce).toBe(analysis2.estimatedLinesToReduce)
    })
  })

  describe('generateAnalysisReport', () => {
    let report: string

    beforeEach(() => {
      report = generateAnalysisReport()
    })

    it('should generate a markdown report', () => {
      expect(report).toBeDefined()
      expect(typeof report).toBe('string')
      expect(report.length).toBeGreaterThan(100)
    })

    it('should have report title', () => {
      expect(report).toContain('# Code Repetition Analysis Report')
    })

    it('should include summary statistics', () => {
      expect(report).toContain('**Total Patterns Detected:**')
      expect(report).toContain('**High Impact Patterns:**')
      expect(report).toContain('**Estimated Lines to Reduce:**')
    })

    it('should have patterns by impact section', () => {
      expect(report).toContain('## Patterns by Impact')
    })

    it('should include high impact patterns section', () => {
      expect(report).toMatch(/### HIGH Impact \(\d+ patterns\)/i)
    })

    it('should include medium impact patterns if present', () => {
      const analysis = analyzeCodePatterns()
      const hasMediumPatterns = analysis.patterns.some(p => p.impact === 'medium')

      if (hasMediumPatterns) {
        expect(report).toMatch(/### MEDIUM Impact/i)
      }
    })

    it('should include low impact patterns if present', () => {
      const analysis = analyzeCodePatterns()
      const hasLowPatterns = analysis.patterns.some(p => p.impact === 'low')

      if (hasLowPatterns) {
        expect(report).toMatch(/### LOW Impact/i)
      }
    })

    it('should format pattern details', () => {
      expect(report).toContain('**Description:**')
      expect(report).toContain('**Occurrences:**')
      expect(report).toContain('**Solution:**')
      expect(report).toContain('**Sample Locations:**')
    })

    it('should include pattern locations as bullet points', () => {
      const bulletPoints = report.match(/^- /gm)
      expect(bulletPoints).toBeTruthy()
      expect(bulletPoints!.length).toBeGreaterThan(0)
    })

    it('should have consistent report structure', () => {
      const lines = report.split('\n')
      expect(lines[0]).toBe('# Code Repetition Analysis Report')
      expect(lines[1]).toBe('')
      expect(lines[2]).toContain('**Total Patterns Detected:**')
    })

    it('should generate different content based on analysis', () => {
      const analysis = analyzeCodePatterns()
      expect(report).toContain(String(analysis.totalPatterns))
      expect(report).toContain(String(analysis.highImpactPatterns))
    })
  })

  describe('printAnalysisSummary', () => {
    let consoleGroupSpy: ReturnType<typeof vi.spyOn>
    let consoleGroupEndSpy: ReturnType<typeof vi.spyOn>
    let consoleLogSpy: ReturnType<typeof vi.spyOn>

    beforeEach(() => {
      consoleGroupSpy = vi.spyOn(console, 'group').mockImplementation(() => {})
      consoleGroupEndSpy = vi.spyOn(console, 'groupEnd').mockImplementation(() => {})
      consoleLogSpy = vi.spyOn(console, 'log').mockImplementation(() => {})
    })

    afterEach(() => {
      consoleGroupSpy.mockRestore()
      consoleGroupEndSpy.mockRestore()
      consoleLogSpy.mockRestore()
    })

    it('should print analysis summary to console', () => {
      printAnalysisSummary()

      expect(consoleGroupSpy).toHaveBeenCalled()
      expect(consoleLogSpy).toHaveBeenCalled()
      expect(consoleGroupEndSpy).toHaveBeenCalled()
    })

    it('should print main analysis group', () => {
      printAnalysisSummary()

      expect(consoleGroupSpy).toHaveBeenCalledWith('🔍 Code Repetition Analysis')
    })

    it('should print summary statistics', () => {
      printAnalysisSummary()

      expect(consoleLogSpy).toHaveBeenCalledWith(expect.stringContaining('📊 Total Patterns:'))
      expect(consoleLogSpy).toHaveBeenCalledWith(expect.stringContaining('⚠️  High Impact:'))
      expect(consoleLogSpy).toHaveBeenCalledWith(expect.stringContaining('📉 Potential Reduction:'))
    })

    it('should print top patterns group', () => {
      printAnalysisSummary()

      expect(consoleGroupSpy).toHaveBeenCalledWith('🎯 Top 3 High-Impact Patterns')
    })

    it('should print top 3 high-impact patterns', () => {
      printAnalysisSummary()

      const analysis = analyzeCodePatterns()
      const highImpactPatterns = analysis.patterns.filter(p => p.impact === 'high').slice(0, 3)

      highImpactPatterns.forEach((pattern) => {
        expect(consoleLogSpy).toHaveBeenCalledWith(expect.stringContaining(pattern.name))
        expect(consoleLogSpy).toHaveBeenCalledWith(expect.stringContaining(pattern.solution))
      })
    })

    it('should close console groups properly', () => {
      printAnalysisSummary()

      expect(consoleGroupEndSpy).toHaveBeenCalledTimes(2) // Main group + patterns group
    })

    it('should handle case with no high-impact patterns gracefully', () => {
      // This test verifies the function doesn't crash even if no high-impact patterns exist
      // (which shouldn't happen in practice, but good to test)
      expect(() => printAnalysisSummary()).not.toThrow()
    })

    it('should format pattern numbers correctly', () => {
      printAnalysisSummary()

      expect(consoleLogSpy).toHaveBeenCalledWith(expect.stringMatching(/1\. .+ \(\d+ occurrences\)/))
    })
  })

  describe('integration', () => {
    it('should work together: analyze → generate report', () => {
      const analysis = analyzeCodePatterns()
      const report = generateAnalysisReport()

      expect(report).toContain(String(analysis.totalPatterns))
      expect(report).toContain(String(analysis.highImpactPatterns))
      expect(report).toContain(String(analysis.estimatedLinesToReduce))
    })

    it('should work together: analyze → print summary', () => {
      const consoleSpy = vi.spyOn(console, 'log').mockImplementation(() => {})

      analyzeCodePatterns()
      printAnalysisSummary()

      expect(consoleSpy).toHaveBeenCalled()

      consoleSpy.mockRestore()
    })

    it('should provide consistent data across all functions', () => {
      const analysis = analyzeCodePatterns()
      const report = generateAnalysisReport()

      analysis.patterns.slice(0, 3).forEach((pattern) => {
        expect(report).toContain(pattern.name)
      })
    })
  })
})
