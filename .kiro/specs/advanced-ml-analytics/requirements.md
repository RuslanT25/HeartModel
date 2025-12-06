# Advanced ML Analytics Requirements

## Introduction

Bu özellik, HeartMVC projesine gelişmiş makine öğrenimi analiz yetenekleri ekler. Feature importance, ROC curves, performance timing ve data preprocessing istatistikleri gibi derinlemesine analiz araçları sağlar.

## Requirements

### Requirement 1: Feature Importance Analysis

**User Story:** As a data scientist, I want to see which features are most important for the Random Forest model's predictions, so that I can understand which health indicators have the greatest impact on heart disease risk.

#### Acceptance Criteria

1. WHEN the evaluation runs THEN the system SHALL calculate feature importance scores using ML.NET's PermutationFeatureImportance
2. WHEN feature importance is displayed THEN it SHALL show both tabular data and a Chart.js bar chart
3. WHEN features are ranked THEN they SHALL be ordered from most to least important
4. WHEN the chart displays THEN it SHALL use appropriate colors and be responsive

### Requirement 2: ROC Curve Visualization

**User Story:** As a medical professional, I want to see the ROC curve for the Logistic Regression model, so that I can visually assess the model's diagnostic performance across different thresholds.

#### Acceptance Criteria

1. WHEN the evaluation runs THEN the system SHALL generate ROC curve data (TPR vs FPR points)
2. WHEN the ROC curve displays THEN it SHALL use Chart.js line chart with appropriate styling
3. WHEN the curve is shown THEN it SHALL include the diagonal reference line (random classifier)
4. WHEN hovering over points THEN it SHALL show threshold, TPR, and FPR values

### Requirement 3: Performance Timing Analysis

**User Story:** As a system administrator, I want to compare training and prediction times between models, so that I can make informed decisions about deployment and resource allocation.

#### Acceptance Criteria

1. WHEN models are evaluated THEN the system SHALL measure training time using Stopwatch
2. WHEN predictions are made THEN the system SHALL measure prediction time for both models
3. WHEN timing data displays THEN it SHALL show milliseconds with appropriate formatting
4. WHEN comparing times THEN it SHALL highlight the faster model for each metric

### Requirement 4: Data Preprocessing Statistics

**User Story:** As a data analyst, I want to see comprehensive statistics about the dataset, so that I can understand data quality and distribution before model training.

#### Acceptance Criteria

1. WHEN data analysis runs THEN the system SHALL detect missing values per column
2. WHEN outliers are analyzed THEN the system SHALL use IQR method for detection
3. WHEN statistics display THEN they SHALL show mean, median, min, max for each numeric column
4. WHEN data info shows THEN it SHALL include row count, column count, and data types

### Requirement 5: Advanced Analytics Dashboard

**User Story:** As a user, I want all advanced analytics in a dedicated dashboard with modern UI, so that I can access comprehensive model insights in one place.

#### Acceptance Criteria

1. WHEN the dashboard loads THEN it SHALL display all analytics in organized sections
2. WHEN charts render THEN they SHALL be responsive and interactive
3. WHEN data loads THEN it SHALL show loading indicators for better UX
4. WHEN errors occur THEN they SHALL be handled gracefully with user-friendly messages