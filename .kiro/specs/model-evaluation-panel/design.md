# Design Document

## Overview

The Model Evaluation Panel will be implemented as a new MVC controller and view that displays comprehensive performance metrics for both Fast Forest and Logistic Regression models. The system will use a dedicated evaluation service to calculate metrics using the same test dataset, ensuring fair comparison between models.

## Architecture

### Components
- **EvaluationController**: Handles HTTP requests for the evaluation panel
- **ModelEvaluationService**: Calculates performance metrics for both models
- **EvaluationViewModel**: Contains structured data for the view
- **Evaluation View**: Displays metrics in a responsive, professional layout

### Data Flow
1. User navigates to `/Heart/Evaluation`
2. EvaluationController calls ModelEvaluationService
3. Service loads both models and evaluates against test dataset
4. Metrics are calculated and returned in EvaluationViewModel
5. View renders the metrics in organized panels

## Components and Interfaces

### ModelEvaluationService
```csharp
public class ModelEvaluationService
{
    public async Task<EvaluationResultViewModel> EvaluateModelsAsync()
    public ModelMetrics EvaluateFastForest(IDataView testData)
    public ModelMetrics EvaluateLogisticRegression(IDataView testData)
    private IDataView LoadAndSplitData()
}
```

### EvaluationResultViewModel
```csharp
public class EvaluationResultViewModel
{
    public ModelMetrics FastForestMetrics { get; set; }
    public ModelMetrics LogisticRegressionMetrics { get; set; }
    public bool HasError { get; set; }
    public string ErrorMessage { get; set; }
}
```

### ModelMetrics
```csharp
public class ModelMetrics
{
    public double Accuracy { get; set; }
    public double Precision { get; set; }
    public double Recall { get; set; }
    public double F1Score { get; set; }
    public double AUC { get; set; }
    public ConfusionMatrixData ConfusionMatrix { get; set; }
}
```

### ConfusionMatrixData
```csharp
public class ConfusionMatrixData
{
    public int TruePositive { get; set; }
    public int TrueNegative { get; set; }
    public int FalsePositive { get; set; }
    public int FalseNegative { get; set; }
}
```

## Data Models

### Test Data Handling
- Use the same random seed for data splitting to ensure consistent test sets
- Split ratio: 80% training, 20% testing (consistent with existing services)
- Cache the test dataset to avoid multiple file reads

### Metrics Calculation
- Use ML.NET's built-in evaluation methods for accuracy and reliability
- Calculate derived metrics (F1 Score) using standard formulas
- Format all metrics to 4 decimal places for consistency

## Error Handling

### Model Loading Errors
- Check for model file existence before evaluation
- Display specific error messages for missing files
- Graceful degradation when one model fails but the other succeeds

### Data Processing Errors
- Validate CSV file format and content
- Handle corrupted or incomplete datasets
- Provide clear error messages for data-related issues

### Calculation Errors
- Wrap evaluation calls in try-catch blocks
- Log detailed errors for debugging
- Show user-friendly error messages in the UI

## Testing Strategy

### Unit Tests
- Test ModelEvaluationService with mock data
- Verify metric calculations against known values
- Test error handling scenarios

### Integration Tests
- Test full evaluation workflow with real models
- Verify view model population
- Test controller error handling

### UI Tests
- Verify responsive design on different screen sizes
- Test metric display formatting
- Validate confusion matrix rendering

## User Interface Design

### Layout Structure
```
Navigation Bar (with Evaluation link)
├── Page Header: "Model Performance Evaluation"
├── Fast Forest Panel
│   ├── Model Info Header
│   ├── Metrics Table (Accuracy, Precision, Recall, F1, AUC)
│   └── Confusion Matrix Table
├── Logistic Regression Panel
│   ├── Model Info Header
│   ├── Metrics Table (Accuracy, Precision, Recall, F1, AUC)
│   └── Confusion Matrix Table
└── Comparison Summary (optional)
```

### Styling Guidelines
- Use Bootstrap cards for model panels
- Color-code metrics (green for good, yellow for moderate, red for poor)
- Responsive grid layout for mobile compatibility
- Professional typography and spacing

### Navigation Integration
- Add "Model Evaluation" link to main navigation
- Use appropriate icons (fas fa-chart-bar)
- Highlight active navigation state

## Performance Considerations

### Caching Strategy
- Cache evaluation results for 1 hour to avoid recalculation
- Invalidate cache when models are retrained
- Use in-memory caching for development, consider Redis for production

### Lazy Loading
- Load models only when evaluation is requested
- Dispose of models after evaluation to free memory
- Use async/await for non-blocking operations

## Security Considerations

### Access Control
- No additional authentication required (same as existing pages)
- Ensure evaluation doesn't expose sensitive training data
- Validate all inputs to prevent injection attacks

### Data Privacy
- Don't display individual data points, only aggregated metrics
- Ensure confusion matrix doesn't reveal sensitive patterns
- Log evaluation requests for audit purposes