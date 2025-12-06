# Advanced ML Analytics Design

## Overview

The Advanced ML Analytics feature extends the existing evaluation system with sophisticated analysis capabilities including feature importance, ROC curves, performance timing, and data preprocessing statistics. The system will use Chart.js for interactive visualizations and provide a comprehensive analytics dashboard.

## Architecture

### Components
- **AdvancedAnalyticsService**: Core service for advanced ML analysis
- **FeatureImportanceAnalyzer**: Calculates and formats feature importance data
- **ROCCurveGenerator**: Generates ROC curve data points
- **PerformanceTimer**: Measures training and prediction times
- **DataStatisticsAnalyzer**: Analyzes dataset statistics and quality
- **AdvancedAnalyticsController**: New controller for analytics dashboard
- **Analytics Views**: Dedicated views for advanced analytics display

### Data Flow
1. User navigates to `/Heart/Analytics`
2. AdvancedAnalyticsService orchestrates all analysis components
3. Feature importance calculated using ML.NET PermutationFeatureImportance
4. ROC curve data generated from Logistic Regression predictions
5. Performance timing measured during model operations
6. Data statistics computed from raw dataset
7. Results formatted and sent to analytics dashboard view

## Data Models

### FeatureImportanceData
```csharp
public class FeatureImportanceData
{
    public string FeatureName { get; set; }
    public double ImportanceScore { get; set; }
    public double StandardDeviation { get; set; }
    public int Rank { get; set; }
}
```

### ROCCurvePoint
```csharp
public class ROCCurvePoint
{
    public double Threshold { get; set; }
    public double TruePositiveRate { get; set; }
    public double FalsePositiveRate { get; set; }
}
```

### PerformanceTimingData
```csharp
public class PerformanceTimingData
{
    public long FastForestTrainingTime { get; set; }
    public long FastForestPredictionTime { get; set; }
    public long LogisticRegressionTrainingTime { get; set; }
    public long LogisticRegressionPredictionTime { get; set; }
}
```

### DataStatistics
```csharp
public class DataStatistics
{
    public int TotalRows { get; set; }
    public int TotalColumns { get; set; }
    public Dictionary<string, ColumnStatistics> ColumnStats { get; set; }
    public Dictionary<string, int> MissingValues { get; set; }
    public Dictionary<string, int> OutlierCounts { get; set; }
}

public class ColumnStatistics
{
    public double Mean { get; set; }
    public double Median { get; set; }
    public double Min { get; set; }
    public double Max { get; set; }
    public string DataType { get; set; }
}
```

### AdvancedAnalyticsViewModel
```csharp
public class AdvancedAnalyticsViewModel
{
    public List<FeatureImportanceData> FeatureImportance { get; set; }
    public List<ROCCurvePoint> ROCCurveData { get; set; }
    public PerformanceTimingData TimingData { get; set; }
    public DataStatistics DataStats { get; set; }
    public bool HasError { get; set; }
    public string ErrorMessage { get; set; }
}
```

## Technical Implementation

### Feature Importance Calculation
- Use ML.NET's PermutationFeatureImportance API
- Calculate importance for each feature in the Random Forest model
- Rank features by importance score
- Include standard deviation for statistical significance

### ROC Curve Generation
- Generate predictions with probability scores from Logistic Regression
- Calculate TPR and FPR for multiple threshold values (0.0 to 1.0 in 0.01 steps)
- Format data for Chart.js line chart visualization
- Include diagonal reference line for random classifier comparison

### Performance Timing
- Use System.Diagnostics.Stopwatch for precise timing
- Measure training time during model creation
- Measure prediction time for single sample predictions
- Compare performance between FastForest and Logistic Regression

### Data Statistics Analysis
- Load raw dataset and analyze each column
- Detect missing values using null/empty checks
- Calculate outliers using IQR method (Q1 - 1.5*IQR, Q3 + 1.5*IQR)
- Compute descriptive statistics (mean, median, min, max)
- Determine data types for each column

## User Interface Design

### Dashboard Layout
```
Navigation Bar (with Analytics link)
├── Page Header: "Advanced ML Analytics Dashboard"
├── Feature Importance Section
│   ├── Importance Table (Feature, Score, Rank)
│   └── Bar Chart (Chart.js)
├── ROC Curve Section
│   ├── ROC Metrics (AUC, Optimal Threshold)
│   └── ROC Curve Chart (Chart.js)
├── Performance Timing Section
│   ├── Training Time Comparison
│   └── Prediction Time Comparison
└── Data Statistics Section
    ├── Dataset Overview (Rows, Columns, Missing Values)
    ├── Column Statistics Table
    └── Outlier Analysis
```

### Chart.js Integration
- Feature Importance: Horizontal bar chart with gradient colors
- ROC Curve: Line chart with smooth curves and hover tooltips
- Responsive design for mobile compatibility
- Interactive legends and zoom capabilities

### Styling Guidelines
- Use consistent color scheme with existing application
- Implement card-based layout for each analytics section
- Add loading spinners for async operations
- Use progressive disclosure for detailed statistics

## Performance Considerations

### Caching Strategy
- Cache feature importance calculations (expensive operation)
- Cache ROC curve data for consistent thresholds
- Invalidate cache when models are retrained
- Use memory caching for development environment

### Optimization Techniques
- Lazy load charts to improve initial page load
- Use async/await for all heavy computations
- Implement pagination for large datasets
- Compress chart data for faster transmission

### Resource Management
- Dispose of ML.NET objects properly
- Limit ROC curve points to reasonable number (100 points)
- Use streaming for large dataset analysis
- Monitor memory usage during statistics calculation

## Error Handling

### Graceful Degradation
- Show partial results if some analytics fail
- Provide fallback visualizations for chart errors
- Display informative error messages for each section
- Allow retry mechanisms for failed operations

### Validation
- Validate model files exist before analysis
- Check dataset integrity before statistics calculation
- Verify Chart.js library availability
- Handle browser compatibility issues

## Security Considerations

### Data Privacy
- Don't expose individual data points in charts
- Aggregate sensitive information appropriately
- Log analytics requests for audit purposes
- Ensure no PII in feature importance displays

### Input Validation
- Validate all user inputs and parameters
- Sanitize data before chart rendering
- Prevent injection attacks in dynamic content
- Implement rate limiting for expensive operations