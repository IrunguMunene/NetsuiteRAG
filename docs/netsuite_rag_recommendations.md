**Permission Matrix:**

| Role | Personal | Team | Organization |
|------|----------|------|--------------|
| End User | View own, Use own | View team, Use team | View, Use |
| Power User | Full control | Full control | View, Use |
| Admin | View all, Use all | Full control | Full control |
| Super Admin | Full control | Full control | Full control |

**Approval Workflow:**

**Step 1: Submission**
- User clicks "Publish as Organization Template"
- Required fields validation
- Automatic quality checks run (T3.6)
- If quality score <70: Soft rejection with improvement suggestions
- If quality score ≥70: Creates review ticket
- Status: Draft → Pending Review

**Step 2: Automated Checks**
- Quality validation against golden questions
- Performance benchmarking
- Schema compatibility verification
- Duplicate detection (similar templates exist?)
- Results attached to review ticket

**Step 3: Admin Review**
- Admin receives notification within 1 hour
- Reviews: Template metadata, Quality scores, Validation results, Comparison with existing templates
- Tests with sample data in sandbox
- Decision points: Approve immediately, Request changes, Reject with feedback, Assign to specialist reviewer

**Step 4: Approval Decision**
- If approved: Status → Approved → Admin can activate immediately or schedule
- If changes requested: Status → Draft, creator notified with feedback
- If rejected: Status → Archived, detailed explanation provided

**Step 5: Activation**
- Admin activates template
- Status: Approved → Active
- Email to submitter: "Your template is now live"
- Template indexed for retrieval
- Announced in weekly digest to organization

**Version Control Workflow:**

**Making Changes to Active Template:**
1. Owner clicks "Edit Template"
2. System creates draft copy (v2.0.0-draft)
3. Owner makes modifications
4. Owner must provide changelog entry
5. System analyzes changes for breaking vs non-breaking
6. If breaking (major version): Requires re-approval
7. If non-breaking (minor/patch): Owner can publish directly
8. On publish: New version goes Active, old version Deprecated
9. Users notified of update with changelog

**Rollback Scenario:**
1. Template v3.0.0 introduced but has issues
2. Admin clicks "Rollback to v2.5.0"
3. System confirms: "This will revert to previous version. Continue?"
4. New version created: v3.0.1 (copy of v2.5.0 structure)
5. Changelog auto-populated: "Rolled back to v2.5.0 due to [reason]"
6. Status: Active (new version), Archived (broken version)
7. All users see rollback immediately

**Governance Reporting:**

**Weekly Admin Digest:**
- Pending approvals: Count and breakdown by submitter
- Recently activated: List of new templates this week
- Quality alerts: Templates with declining scores
- Usage stats: Top 10 most-used templates
- Deprecation warnings: Templates needing replacement
- Orphaned templates: Owners who left organization

**Monthly Quality Report:**
- Template library health: Total count, active vs archived, avg quality score
- Approval metrics: Submissions, approval rate, avg review time
- Usage analytics: Reuse rates by template, user adoption
- Performance trends: Execution times, success rates
- Recommendations: Templates to retire, gaps to fill with new templates

**Compliance Audit Trail:**
- Every template action logged: Who, what, when, why
- Immutable audit log (signed, append-only)
- Queryable by: Template ID, user, date range, action type
- Export capability for auditors
- Retention: 7 years for financial query templates

**User Experience Enhancements:**

**Template Browsing:**
- Badge system: "Official" (org-approved), "Popular" (high usage), "New" (last 30 days), "Beta" (pending feedback)
- Quality indicators: Star rating based on score, confidence level, success rate
- Ownership display: "Maintained by Finance Team" or "Created by Jane Doe"
- Last updated timestamp
- Version history link

**Using Templates:**
- Banner shows: "Using template: Revenue Monthly Trend (Official, v2.3.1)"
- Quality badge visible
- Link to template details: Structure, changelog, owner contact
- Option to "Use LLM instead" if user distrusts template
- Automatic migration: If template deprecated, suggest replacement

**Benefits Summary:**

| Metric | Before Governance | After Governance | Improvement |
|--------|-------------------|------------------|-------------|
| Template quality (avg score) | 72/100 | 87/100 | 21% increase |
| User trust in templates | 45% | 78% | 73% increase |
| Template reuse rate | 45% | 68% | 51% increase |
| Bad template reports | 12/month | 2/month | 83% reduction |
| Template library clutter | 250 templates | 85 active templates | 66% reduction |
| Admin oversight effort | 20 hrs/month | 8 hrs/month | 60% reduction |
| Compliance audit readiness | Poor | Excellent | Full traceability |

---

## Recommendation 9: Performance Testing Integration {#recommendation-9}

### Why This Recommendation

The architecture defines clear performance budgets (planning <1s P95, first page 2-4s, etc.) but lacks systematic validation:

**Current Gaps:**

**No Regression Detection:**
- Code changes may degrade performance unknowingly
- Only discover issues in production after user complaints
- No gate preventing slow code from deploying
- Performance degradation accumulates over time

**Capacity Unknown:**
- Don't know how many concurrent users system can handle
- Infrastructure scaling decisions based on guesswork
- No validation of 50 concurrent searches claim
- Peak load behavior untested

**Unrealistic Testing:**
- Manual testing with simple queries doesn't represent production
- Small data volumes don't reveal scale issues
- Single-user testing misses concurrency problems
- Missing edge cases: Large result sets, complex queries, slow NetSuite

**No Performance Culture:**
- Developers unaware of performance impact of changes
- No visibility into performance trends
- No accountability for meeting budgets
- Performance treated as afterthought, not requirement

### What This Improves

**Proactive Issue Prevention:**
- Catch performance regressions before production
- Block deployments that violate budgets
- Identify bottlenecks during development
- Test at realistic scale with real-world scenarios

**Confident Scaling:**
- Know exact system capacity (users, queries/sec)
- Data-driven infrastructure decisions
- Predictable performance under load
- Cost-effective capacity planning

**Quality Assurance:**
- Automated performance validation in CI/CD
- Trend tracking shows degradation early
- SLA compliance verification
- Customer experience protection

**Development Culture:**
- Performance as first-class requirement
- Immediate feedback on performance impact
- Clear accountability through metrics
- Optimization opportunities identified

### Implementation Guidance

#### New User Stories

**US-DEV-01**: As a developer, when I submit a pull request, automated performance tests run against realistic workloads, failing the build if my changes cause P95 latency to exceed budgets or increase by more than 20%.

**US-OPS-06**: As operations, I can run quarterly load tests simulating 2x projected peak capacity to validate our infrastructure can handle growth and identify scaling bottlenecks before they impact users.

**US-QA-01**: As QA, I have a library of 1000+ realistic test queries spanning all complexity levels, which I use for pre-release performance validation ensuring all user scenarios are tested at scale.

#### New Tasks

**T0.9 • Performance Test Framework Setup**
- **Purpose:** Establish K6/Gatling/JMeter infrastructure for load testing
- **Inputs:** Performance budgets from PRD, test environment access, baseline queries
- **Output:** Executable test framework with scenario library and reporting
- **Owner:** QA + DevOps
- **Dependencies:** T0.2 (Environments)
- **Acceptance Criteria:**
  - Framework chosen based on: JavaScript familiarity (K6), scalability needs, cloud integration
  - 10 baseline scenarios covering: Smoke (1 user), Load (50 users), Stress (100-200 users), Spike (sudden 200 users), Soak (baseline for 1+ hour)
  - Parameterized for different load levels without code changes
  - Results export to CI dashboard (Grafana/Datadog)
  - Pass/fail thresholds configurable per test
  - Can run locally and in CI/CD pipeline

**T7.15 • Synthetic Load Generation**
- **Purpose:** Create realistic test queries and expected results
- **Inputs:** Golden questions, production query anonymized samples, query complexity matrix
- **Output:** 1000+ test queries with metadata (complexity, expected time, result rows)
- **Owner:** QA
- **Dependencies:** T0.6 (Golden Questions)
- **Acceptance Criteria:**
  - Query distribution: 40% simple, 40% moderate, 20% complex
  - Coverage: All record types, major subsidiaries, all date range patterns, disambiguation scenarios
  - Expected results defined for validation
  - Updateable from production anonymized data quarterly
  - Includes edge cases: Empty results, very large results (50K+ rows), timeout scenarios
  - Tagged with: Complexity, record type, expected latency, expected API units

**T7.16 • Performance CI Gate**
- **Purpose:** Block deployments that violate performance budgets
- **Inputs:** T0.9 test suite, baseline performance metrics, tolerance thresholds
- **Output:** CI job that fails builds on performance regression
- **Owner:** DevOps
- **Dependencies:** T0.9 (Framework), T0.3 (CI/CD)
- **Acceptance Criteria:**
  - Runs on every PR to main branch
  - Tests subset of scenarios (smoke + critical paths) for speed (15-20 min)
  - Fail conditions: P95 planning >1.2s (20% buffer over 1s budget), P95 first page >5s (25% buffer over 4s budget), P95 per-page >2.5s, Error rate >5%, Validation failure rate >10%
  - Trend comparison: Fails if P95 increases >20% vs previous build
  - Results posted as PR comment with breakdown
  - Override capability for intentional changes with approval

**T7.17 • Comprehensive Load Testing Protocol**
- **Purpose:** Pre-release validation of system capacity
- **Inputs:** Projected user counts (50 concurrent, 500 daily), usage patterns, growth projections
- **Output:** Quarterly load test runbook, execution schedule, capacity reports
- **Owner:** QA + DevOps
- **Dependencies:** T0.9 (Framework), T7.15 (Test Data)
- **Acceptance Criteria:**
  - Quarterly execution schedule (pre-major releases, Q4 peak season prep)
  - Test scenarios: Sustained load (2x current peak for 1 hour), Stress test (incremental ramp to breaking point), Spike test (10x load for 5 minutes), Soak test (baseline for 24 hours), Isolation tests (single component stress)
  - Runbook includes: Pre-test checklist, Execution steps, Monitoring points, Success criteria, Rollback procedures
  - Reports document: Capacity limits found, Bottlenecks identified, Scalability recommendations, Infrastructure costs at scale
  - Presented to engineering and leadership quarterly

**T7.X • Performance Monitoring Dashboard**
- **Purpose:** Real-time and historical performance visibility
- **Inputs:** Application metrics, test results, user experience data
- **Output:** Grafana dashboard with SLA tracking and alerts
- **Owner:** DevOps
- **Dependencies:** T7.3 (Observability), T7.16
- **Acceptance Criteria:**
  - Real-time metrics: Current P50/P95/P99 latency by endpoint, Error rates, Active users
  - Historical trends: 7/30/90 day performance graphs, Week-over-week comparisons
  - Budget compliance: Visual indicators showing green (<budget), yellow (80-100% of budget), red (over budget)
  - Test results: CI performance test history, Load test summaries
  - Alerts: P95 exceeds budget for 10 minutes, Error rate >3% for 5 minutes, Capacity >80% for 15 minutes

#### Architecture Additions

**Test Scenario Definitions:**

**Smoke Test (Minimal Load):**
- Purpose: Verify basic functionality
- Load: 1 virtual user
- Duration: 1 minute
- Queries: 10 simple queries
- Success: All queries complete, no errors
- Use: Every commit, pre-deployment sanity check

**Load Test (Expected Peak):**
- Purpose: Validate normal operating capacity
- Load: 50 concurrent users
- Duration: 20 minutes (5 min ramp-up, 10 min sustain, 5 min ramp-down)
- Queries: 500 total (mix of simple/moderate/complex)
- Success: P95 within budgets, error rate <1%
- Use: PR validation, weekly regression

**Stress Test (Find Breaking Point):**
- Purpose: Identify maximum capacity
- Load: Ramp from 0 → 200 users over 15 minutes
- Duration: 30 minutes total
- Queries: Continuous until failure or stabilization
- Success: Document breaking point, graceful degradation
- Use: Quarterly capacity planning

**Spike Test (Sudden Load):**
- Purpose: Test system response to traffic surges
- Load: 5 users → 200 users in 10 seconds
- Duration: 10 minutes (spike maintained for 5 min)
- Queries: Burst of 500 concurrent queries
- Success: No crashes, auto-scaling works, recovery time <2 min
- Use: Pre-launch of new features, marketing campaigns

**Soak Test (Stability):**
- Purpose: Identify memory leaks, resource exhaustion
- Load: 25 concurrent users (moderate continuous load)
- Duration: 24 hours
- Queries: ~5000 total over duration
- Success: No degradation over time, no memory growth, no connection leaks
- Use: Pre-major releases, infrastructure changes

**Performance Budget Tracking:**

**Per-Endpoint Budgets:**
- POST /v1/queries (planning): P95 <1s, P99 <2s
- POST /v1/queries/{id}/resolve: P95 <500ms
- GET /v1/queries/{id}/stream (first page): P95 <4s, P99 <6s
- GET /v1/queries/{id}/stream (per page): P95 <2s
- GET /v1/templates: P95 <200ms
- POST /v1/exports: P95 <30s for <50K rows

**System-Wide Budgets:**
- End-to-end query (plan → first result): P95 <6s
- Template matching: P95 <200ms
- Disambiguation resolution: P95 <500ms
- Memory per query: <100MB
- Concurrent queries supported: 50 minimum

**CI Performance Test Structure:**

```
Performance Test Suite (15-20 min runtime)
├── Smoke Test (2 min)
│   ├── 10 simple queries
│   └── Verify: No errors, basic latency
├── Critical Path Test (8 min)
│   ├── Planning performance (50 queries)
│   ├── Validation performance (50 queries)
│   ├── Streaming performance (20 queries)
│   └── Template matching (30 queries)
├── Load Simulation (8 min)
│   ├── 10 concurrent users
│   ├── Mixed query complexity
│   └── Verify: P95 within budgets
└── Results Analysis (2 min)
    ├── Compare to baseline
    ├── Calculate regressions
    ├── Generate report
    └── Pass/Fail decision
```

**Load Test Report Structure:**

**Executive Summary:**
- Test date, duration, objectives
- Peak load achieved: X concurrent users, Y queries/second
- Overall result: PASS/FAIL
- Key findings: Bottlenecks, capacity limits, recommendations

**Performance Metrics:**
- Latency distributions (P50/P95/P99) per endpoint
- Error rates by type
- Throughput: Queries per second sustained
- Resource utilization: CPU, memory, network, database

**Capacity Analysis:**
- Current capacity: 50 concurrent users at X queries/second
- Breaking point: 187 concurrent users (NetSuite API throttling)
- Headroom: 274% (can grow to 187 from 50 baseline)
- Scaling recommendations: Add 2 API servers for 100-user target

**Bottlenecks Identified:**
1. NetSuite API rate limiting at 187 concurrent users
2. Redis connection pool exhaustion at 150 concurrent users
3. Ollama GPU memory at 200 concurrent planning requests

**Recommendations:**
1. Increase Redis connection pool: 50 → 200 connections
2. Deploy second Ollama instance for horizontal scaling
3. Implement request queuing when approaching NetSuite limits
4. Consider NetSuite API tier upgrade for >100 users

**Cost Projections:**
- Current (50 users): $2,500/month infrastructure
- Target (100 users): $4,200/month (2x API servers, Redis upgrade)
- Max (187 users): $7,800/month (full scaling)

**Alerting Thresholds:**

**Critical (Immediate Response):**
- P95 exceeds budget by 50% (e.g., 1.5s when budget is 1s)
- Error rate >5% for 5 minutes
- Service down or unreachable
- Memory usage >90% for 10 minutes

**Warning (Investigation Required):**
- P95 exceeds budget by 20% for 15 minutes
- Error rate 2-5% for 10 minutes
- Performance degradation trend (10% slower week-over-week)
- Capacity >80% for 30 minutes

**Info (Monitoring):**
- Performance trends approaching budgets
- Resource utilization patterns
- Query complexity shifts
- Cache hit rate changes

**Performance Culture:**

**Developer Guidelines:**
- "Performance is a feature" - treat as requirement, not optimization
- Profile code locally before submitting PR
- Review performance test results in PR checks
- Ask: "How will this behave at 10x scale?"
- Consider: Caching opportunities, query optimization, async processing

**Review Process:**
- PR reviewer checks: Performance test results, Latency impact, Resource usage patterns
- Approval requires: Tests pass, No unexplained regressions, Justification for intentional slowdowns
- Documentation: Performance implications noted in changelog

**Continuous Improvement:**
- Monthly performance review meetings
- Quarterly optimization sprints
- Track performance technical debt
- Celebrate performance wins

**Benefits Quantified:**

| Metric | Before Testing | After Testing | Improvement |
|--------|---------------|---------------|-------------|
| Production incidents (performance) | 8/quarter | 1/quarter | 88% reduction |
| P95 budget compliance | 60% of time | 95% of time | 58% improvement |
| Capacity confidence | Unknown | 187 users documented | Full visibility |
| Regression detection | Post-production | Pre-deployment | Shift-left |
| Performance culture | Reactive | Proactive | Cultural change |
| Scaling costs | Overprovisioned 200% | Right-sized +30% | 57% savings |

---

## Recommendation 10: Multi-Tenant Cost Attribution {#recommendation-10}

### Why This Recommendation

The architecture mentions tenant isolation but completely lacks cost visibility and management:

**Current Blind Spots:**

**No Cost Tracking:**
- Unknown which tenants consume most NetSuite API units
- Can't identify expensive query patterns
- No visibility into per-user or per-department costs
- Unable to forecast infrastructure costs based on usage

**Resource Allocation Issues:**
- All tenants share resources equally regardless of usage
- Heavy users subsidized by light users
- No ability to prioritize or throttle based on consumption
- Risk of "noisy neighbor" problems

**Business Challenges:**
- Can't implement chargeback to departments
- No data for pricing decisions (if commercializing)
- Cannot justify infrastructure investments with usage data
- Budget planning based on guesswork

**Fairness & Governance:**
- No quotas or fair-use limits
- Unlimited consumption possible
- No consequences for inefficient query patterns
- Cannot enforce resource policies

### What This Improves

**Financial Visibility:**
- Complete cost attribution by tenant, department, user
- Understand true cost per query and per report
- Identify optimization opportunities
- Data-driven budget forecasting

**Fair Resource Allocation:**
- Implement usage-based quotas
- Priority access for critical tenants/users
- Throttling for excessive consumption
- SLA tiers based on usage levels

**Business Enablement:**
- Enable chargeback/showback to departments
- Support for commercialization with usage-based pricing
- ROI demonstration for system investment
- Capacity planning based on actual consumption

**Optimization Incentives:**
- Users aware of resource consumption
- Expensive patterns identified and addressed
- Behavioral change through visibility
- Cost consciousness culture

### Implementation Guidance

#### New User Stories

**US-FIN-01**: As a finance administrator, I can view detailed monthly cost attribution reports showing NetSuite API consumption, compute resources, and storage by tenant, department, and user, enabling accurate chargeback to business units.

**US-NS-06**: As a system administrator, I can set usage quotas per tenant (queries per day, API units per month, storage limits) and receive alerts when tenants approach or exceed quotas, allowing proactive management.

**US-USER-01**: As a user, I can view my personal resource consumption dashboard showing queries executed, API units used, and my percentage of team quota, helping me understand my usage patterns.

#### New Tasks

**T7.18 • Cost Metering Service**
- **Purpose:** Track all resource consumption with tenant/user attribution
- **Inputs:** API calls, compute time, storage usage, network egress, export volumes
- **Output:** Time-series cost metrics in InfluxDB/Prometheus
- **Owner:** Backend
- **Dependencies:** T7.3 (Observability)
- **Acceptance Criteria:**
  - Meter NetSuite API units consumed per query
  - Track LLM token usage (prompt + completion + embeddings)
  - Record query execution time (compute cost proxy)
  - Measure storage: Templates, exports, audit logs
  - Network egress for large exports
  - Sub-second metering latency (async, non-blocking)
  - Data retention: 90 days detailed, 2 years aggregated
  - Tagging: Tenant ID, user ID, department, query type, template usage

**T7.19 • Cost Attribution Dashboard**
- **Purpose:** Visualize and analyze costs by multiple dimensions
- **Inputs:** T7.18 metrics, cost rate configuration, organizational hierarchy
- **Output:** Grafana/custom dashboard with drill-down and filtering
- **Owner:** DevOps + Frontend
- **Dependencies:** T7.18 (Metering)
- **Acceptance Criteria:**
  - Views: By tenant, by department, by user, by query type, by time period
  - Time series: Daily/weekly/monthly cost trends
  - Top consumers: Top 10 tenants/users/queries by cost
  - Cost breakdown: API units vs compute vs storage vs network
  - Comparison: Month-over-month, year-over-year, budget vs actual
  - Forecasting: Project next month cost based on trends
  - Export: CSV/PDF reports for finance

**T7.20 • Resource Quotas & Throttling**
- **Purpose:** Enforce fair-use limits and prevent resource exhaustion
- **Inputs:** Quota configuration per tenant/user, T7.18 consumption data
- **Output:** Rate limiter with quota enforcement, graceful degradation
- **Owner:** Backend
- **Dependencies:** T7.6 (Rate Limiting), T7.18 (Metering)
- **Acceptance Criteria:**
  - Quota types: Queries per day, API units per month, Exports per day, Storage per tenant
  - Soft limits: Warning at 80% consumption
  - Hard limits: Block new requests at 100%
  - Grace period: Allow 110% for occasional overages
  - Reset schedule: Daily/monthly based on quota type
  - User notifications: Email at 80%, 90%, 100%
  - Admin override: Temporary quota increases
  - Self-service: Users can request quota increases with justification

**T7.21 • Cost Allocation Reports**
- **Purpose:** Automated chargeback/showback reports for finance
- **Inputs:** T7.18 aggregated data, cost rates, organizational structure
- **Output:** Monthly PDF/CSV reports with cost breakdowns
- **Owner:** Backend
- **Dependencies:** T7.18 (Metering)
- **Acceptance Criteria:**
  - Scheduled generation: 1st business day of each month for previous month
  - Cost rates configurable: $/API unit ($0.0001), $/compute-hour ($0.05), $/GB-storage ($0.10/month), $/LLM-token ($0.000002)
  - Breakdown by: Tenant, Department, User, Query type (simple/template/composition)
  - Includes: Summary (total cost), Details (line items), Trends (vs previous months), Recommendations (optimization opportunities)
  - Email delivery to: Finance stakeholders, Department heads, Tenant administrators
  - Archive: 7 years for audit compliance

**T7.X • Usage Analytics & Optimization**
- **Purpose:** Identify cost optimization opportunities
- **Inputs:** Cost attribution data, query patterns, performance metrics
- **Output:** Monthly recommendations report
- **Owner:** Backend/MLE
- **Dependencies:** T7.18, T7.19
- **Acceptance Criteria:**
  - Identify: Inefficient queries (high API units per row), Users with low template reuse (optimization opportunity), Expensive composition queries, Redundant queries (same query run multiple times)
  - Recommend: Query pattern improvements, Template creation suggestions, User training needs, Infrastructure optimization
  - Prioritize: By potential savings (highest first)
  - Track: Implementation of recommendations, Savings realized

#### Architecture Additions

**Cost Metric Schema:**

```
cost_metrics table:
- timestamp
- tenant_id
- user_id
- query_id
- query_type (simple/template/composition)
- record_type
- resources:
  - netsuite_api_units
  - llm_tokens_prompt
  - llm_tokens_completion
  - llm_tokens_embedding
  - compute_ms
  - storage_mb (exports/templates/logs)
  - network_gb (egress)
- costs:
  - netsuite_cost
  - compute_cost
  - storage_cost
  - llm_cost
  - network_cost
  - total_cost
- metadata:
  - rows_returned
  - execution_time_ms
  - export_type
  - template_used
```

**Quota Schema:**

```
tenant_quotas table:
- tenant_id
- period (daily/monthly)
- limits:
  - queries_per_period
  - api_units_per_period
  - exports_per_period
  - storage_gb
- consumed:
  - queries
  - api_units
  - exports
  - storage_gb
- status:
  - queries_remaining
  - api_units_remaining
  - percent_used
  - warning_sent (boolean)
  - hard_limit_reached (boolean)
- reset_at
- created_at
- updated_at
```

**Cost Rate Configuration:**

```yaml
cost_rates:
  netsuite:
    per_api_unit: 0.0001  # $0.0001 per unit
    
  compute:
    per_ms: 0.00001  # $0.01 per 1000 seconds
    
  storage:
    per_gb_month: 0.10  # $0.10 per GB per month
    templates: 0.05
    exports: 0.08
    audit_logs: 0.12  # Higher due to retention requirements
    
  llm:
    per_token: 0.000002  # $0.002 per 1M tokens
    embedding_per_token: 0.0000001
    
  network:
    egress_per_gb: 0.09  # $0.09 per GB egress
    
  multipliers:
    composition_queries: 1.5  # 50% premium for complexity
    priority_users: 0.8  # 20% discount for strategic users
```

**Metering Implementation:**

**On Query Execution:**
1. Record start state: timestamp, user context, query type
2. Instrument each phase:
   - Planning: LLM tokens consumed
   - Validation: Compute time
   - Execution: NetSuite API units, rows fetched
   - Export: Storage created, network egress
3. Calculate costs using rate configuration
4. Write metric to time-series database (async, non-blocking)
5. Update quota consumption atomically
6. Check quota limits, return 429 if exceeded

**Quota Enforcement:**

```
Quota Check Flow:
1. Request arrives for user in tenant
2. Load current quota state from cache
3. Check: consumed + 1 <= limit?
4. If yes: Allow, increment consumed
5. If no:
   - Check grace period: consumed <= limit * 1.1?
   - If in grace: Allow with warning header
   - If over grace: Return 429 with retry-after
6. Update quota state in cache and DB
```

**Cost Attribution Dashboard Views:**

**Executive View (CFO/Finance):**
- Total monthly cost: $12,450
- Cost by tenant: Bar chart, top 5
- Cost trend: Line chart, last 12 months
- Cost per query: Average $0.15
- Forecast: Next month projection $13,200
- Budget status: 94% of $15,000 budget

**Department View (Department Head):**
- My department cost: $2,340
- Cost by user: List with usage details
- Cost breakdown: API (60%), Compute (25%), Storage (10%), LLM (5%)
- Top expensive queries: Link to query details
- Optimization opportunities: Ranked list
- Quota status: 78% of monthly allocation

**User View (Individual User):**
- My usage this month: $47.50
- Queries run: 245
- API units consumed: 12,400
- My team rank: #3 of 12 (usage)
- Query efficiency: 0.19 API units per row (vs team avg 0.25)
- Recommendations: "Consider using templates more (current 40% vs team 65%)"

**Query-Level Cost Breakdown:**

```
Query ID: qry_123456
User: jane.doe@company.com
Timestamp: 2024-10-15 14:32:18
Query Type: Composition

Costs:
├── Planning
│   ├── LLM tokens: 1,245 @ $0.000002 = $0.0025
│   ├── Compute: 850ms @ $0.00001 = $0.0085
│   └── Subtotal: $0.011
├── Execution
│   ├── NetSuite API: 450 units @ $0.0001 = $0.045
│   ├── Compute: 3,200ms @ $0.00001 = $0.032
│   └── Subtotal: $0.077
├── Export
│   ├── Storage: 12MB @ $0.08/GB = $0.001
│   ├── Network: 12MB @ $0.09/GB = $0.001
│   └── Subtotal: $0.002
└── Total: $0.090

Efficiency Metrics:
- Rows returned: 3,450
- Cost per row: $0.000026
- API units per row: 0.13 (Good - below avg 0.20)
- Template reused: No (could save ~30% with template)
```

**Cost Optimization Recommendations:**

**Report Format:**
```
Monthly Optimization Report - September 2024
Generated: October 1, 2024

Top Opportunities (Estimated Monthly Savings):

1. Enable Template Reuse for Power Users ($1,200/month)
   - Users: jane.doe (45 similar queries), john.smith (38 similar)
   - Current cost: $2,100/month on repetitive queries
   - With templates: $900/month (57% savings)
   - Action: Training session + template creation

2. Optimize Date Ranges ($450/month)
   - 127 queries with 3+ year date ranges
   - Average cost: $0.25 vs typical $0.10
   - Recommendation: Default to 1-year range, allow override
   - Action: Update UI with sensible defaults

3. Reduce Composition Query Complexity ($320/month)
   - 23 composition queries averaging $2.50 each
   - Many combinable into single templates
   - Recommendation: Pre-build common compositions
   - Action: Create 5 new composition templates

4. Right-Size Export Formats ($180/month)
   - 45% of exports <1K rows using async (overkill)
   - Recommendation: Tune export tier th**T7.X • Prompt Version Performance Tracking**
- **Purpose:** Compare different prompt template versions to identify optimal prompts
- **Inputs:** T7.12 logs tagged with prompt version hash
- **Output:** A/B test dashboard comparing prompt performance
- **Owner:** MLE
- **Dependencies:** T7.12, T2.3
- **Acceptance Criteria:**
  - Track success rate, latency, token count by prompt version
  - Statistical significance testing (minimum 100 samples per variant)
  - Side-by-side comparison of validation layer pass rates
  - Identify which prompt versions work best for query complexity levels
  - Automatic promotion of winning prompt after significance threshold
  - Rollback capability if new prompt underperforms

#### Architecture Additions

**Decision Log Schema:**

Comprehensive logging captures entire decision pipeline:

**Core Identification:**
- Query ID (correlation across systems)
- Timestamp (ISO 8601 with microseconds)
- User ID, Tenant ID (for analysis and compliance)
- Session ID (track user journey)

**Input Context:**
- Natural language query text
- Query character count, word count
- User's recent query history (last 5)
- User preferences and saved filters
- User role and permissions

**RAG Retrieval Details:**
- **Fields Retrieved:** Field name, Record type, Similarity score, Source (standard/custom), Whether used in final plan
- **Custom Descriptors:** Field ID, Alias matched, Business context text, Similarity score, Usage in plan
- **Glossary Terms:** Term, Matched synonym, Definition, Similarity score, Contextual relevance flag
- **Exemplars:** Exemplar ID, Description, Similarity score, Influence score (0-1 indicating how much it shaped the plan)
- **Retrieval Performance:** Vector search latency, Total items retrieved, Items actually used in prompt, Token count of RAG context

**LLM Interaction:**
- Model identifier and version
- Prompt template ID and version hash
- Complete prompt text (potentially truncated for storage)
- Model parameters: Temperature, Top_p, Max tokens, Stop sequences
- Token counts: Prompt tokens, Completion tokens, Total
- Timing: Prompt build time, Model inference time, Response parsing time, Total latency
- Raw model response (before parsing)
- Grammar/constraints used

**Planning Output:**
- Generated SavedSearchPlan (complete JSON)
- Query complexity classification (simple/moderate/complex)
- Confidence score (if model provides log probabilities)
- Alternative plans considered (if applicable)
- Interpretations extracted: Entities, Metrics, Filters, Temporal references, Groupings

**Validation Pipeline:**
- Layer 1 (JSON): Result, Duration, Error details
- Layer 2 (Schema): Result, Duration, Cache hit/miss, Fields validated, Errors
- Layer 3 (Operators): Result, Duration, Incompatibilities found
- Layer 4 (Guardrails): Result, Duration, Violations, Warnings
- Semantic Layer: Result, Duration, Issues found, Warnings
- Overall: Valid yes/no, Total validation time

**Execution Results (if executed):**
- Start and completion timestamps
- Execution time in milliseconds
- Rows returned, Pages fetched
- NetSuite API units consumed
- Strategy used (RESTlet/REST/SOAP)
- Success/failure status
- Error messages if failed

**User Feedback:**
- Rating (thumbs up/down)
- Issue category if negative
- Free-form comment
- Timestamp of feedback

**Performance Metrics:**
- End-to-end latency breakdown
- Planning phase duration
- Validation phase duration
- Execution phase duration

**Storage Strategy:**

**Hot Storage (Elasticsearch - 90 days):**
- Full decision logs with all fields
- Optimized for search and aggregation
- Indices partitioned by month
- Full-text search on query text and errors

**Warm Storage (S3 - 1 year):**
- Compressed JSON logs
- Queryable via Athena for analysis
- Cost-optimized storage tier
- Used for monthly/quarterly reviews

**Cold Archive (Glacier - 7 years):**
- Compliance-flagged queries only
- Financial or regulated data queries
- Immutable audit trail
- Encrypted with key rotation

**Explainability UI Components:**

**Section 1: Query Interpretation**
- Original query displayed prominently
- Keywords highlighted with color coding: Entities (blue), Metrics (green), Filters (yellow), Temporal (purple)
- Mapping table: "You said" → "We understood"
  - "vendor spend" → "SUM(transaction.amount) WHERE type=vendor"
  - "Q4 2024" → "period = 'Q4 FY2024'"
  - "ACME US" → "subsidiary = 'ACME Corp US East'"

**Section 2: Knowledge Sources**
- Tabbed interface showing different RAG sources
- **Tab 1 - Fields Used:** List of top fields with confidence bars
- **Tab 2 - Similar Queries:** Exemplars that influenced plan with similarity %
- **Tab 3 - Business Terms:** Glossary terms that provided context
- **Tab 4 - Custom Fields:** Any custom field descriptors used
- Each item shows: Name/description, Relevance score, How it was used

**Section 3: Model Performance**
- Which model was used (Llama 3.1 8B, Mistral, etc.)
- How long it took (breakdown: planning, validation, execution)
- Token usage (displayed only if user is admin/power user)
- Confidence score with visual indicator

**Section 4: Validation & Quality**
- Checklist showing all validation layers passed
- Green checkmarks for passed, red X for failed with details
- Performance metrics: Latency by phase with bar chart
- NetSuite API units consumed

**Section 5: Confidence & Trust**
- Overall confidence score (if available): 0-100% with explanation
- Factors contributing to confidence
- Number of similar successful queries
- Template quality score if template was used
- User can click "Regenerate with fresh LLM" if confidence low

**RAG Attribution Dashboard:**

**Exemplar Effectiveness View:**
- Table with columns: Exemplar ID, Description, Times Used, Success Rate, Avg Influence, Validation Pass Rate, Recommendation
- Color coding: Green (>80% success), Yellow (60-80%), Red (<60%)
- Sortable by any column
- Drill-down to see specific queries where exemplar was used
- Actions: Edit, Disable, Delete exemplar

**Glossary Term Analysis:**
- Which terms are retrieved frequently but never used (low value)
- Which terms have high usage and high success rates (valuable)
- Terms with mismatches between retrieval and relevance
- Suggested additions based on query patterns not covered

**Custom Field Descriptor Health:**
- Fields with enrichment (aliases, business context) vs without
- Success rate difference between enriched and non-enriched
- Most commonly queried custom fields
- Fields needing enrichment based on usage

**Prompt Performance Comparison:**

**A/B Test Dashboard:**
- Current prompt version vs candidate version
- Traffic split percentage
- Success metrics side-by-side:
  - Validation pass rate
  - Average latency
  - Token efficiency (tokens per successful plan)
  - User feedback sentiment
- Statistical significance indicator
- Recommendation: Keep current, switch to candidate, continue testing

**Version History:**
- Timeline of prompt versions deployed
- Performance trend over time
- Rollback capability to previous versions
- Change log showing what was modified

**Benefits Summary:**

| Stakeholder | Key Benefit | Impact |
|-------------|-------------|--------|
| Users | Transparent decision-making | Increased trust, easier debugging |
| Admins | Complete audit trail | Compliance readiness, issue resolution |
| ML Engineers | Data-driven optimization | 20-30% quality improvement via prompt tuning |
| Product Team | Usage insights | Feature prioritization, UX improvements |
| Support | Debug capability | 50% faster issue resolution |
| Finance | Cost visibility | 15-20% token reduction through optimization |

---

## Recommendation 7: Semantic Query Validation Layer {#recommendation-7}

### Why This Recommendation

Current validation (Layers 1-4 in T2.4-T2.7) focuses exclusively on syntactic and schema correctness but completely misses semantic and logical issues:

**Current Validation Coverage:**
- **Layer 1:** JSON structure valid
- **Layer 2:** Fields exist in schema
- **Layer 3:** Operators compatible with field types
- **Layer 4:** Guardrails (posting=true, row limits, etc.)

**Critical Gaps:**

**Temporal Logic Issues:**
- "Q4 2024 revenue in 2023" passes validation but makes no sense
- "Last month" on January 1st is ambiguous (December 2024 vs November?)
- End date before start date (2024-12-01 to 2024-01-01)
- Future dates in historical queries
- Fiscal period not matching specified year

**Entity Relationship Violations:**
- Querying vendor fields on customer record type
- Customer fields on vendor transactions
- Incompatible dimensions (employee department + vendor category)
- Subsidiary-specific fields without subsidiary filter

**Business Logic Contradictions:**
- Debit filter + credit filter on overlapping accounts (likely empty results)
- Closed period with modification intent (read-only)
- Posting=true without period filter (will be very slow)
- Account type mismatch (expense account in revenue query)

**Common User Mistakes:**
- Very short date range on first of month ("last week" = 1 day)
- Wildcard searches with <3 characters (performance killer)
- Missing critical filters for multi-subsidiary orgs (pulls all data)
- Unrealistic expectations (5 years of transaction detail)

**User Experience Impact:**
- Issues discovered only after execution (wasted time and API units)
- Empty results with no explanation why
- Extremely slow queries that could have been prevented
- Frustration from logically impossible requests succeeding syntactically

### What This Improves

**Proactive Error Prevention:**
- Catch logical inconsistencies before NetSuite execution
- Save 10-15% of queries from futile execution
- Prevent wasted API units on impossible queries
- Reduce average query cycle time by 30% (no failed attempts)

**Enhanced User Experience:**
- Clear explanations of why combinations don't make sense
- Educational warnings help users learn business logic
- Suggestions for fixes rather than just rejecting
- Confidence that validated queries will succeed

**Resource Efficiency:**
- Avoid expensive NetSuite calls for queries destined to fail
- Reduce load on NetSuite infrastructure
- Lower API consumption by 10-15%
- Faster average response time

**Educational Value:**
- Warnings teach users about business rules and data relationships
- Build institutional knowledge through inline guidance
- Reduce repeat mistakes through explanatory messages
- Self-service improvement without training sessions

**Data Quality:**
- Prevent nonsensical queries from generating misleading reports
- Enforce business logic consistency
- Reduce "garbage in, garbage out" scenarios
- Improve trust in system outputs

### Implementation Guidance

#### New User Stories

**US-FA-08**: As an analyst, when I construct a query with logically inconsistent parameters (like Q4 2024 data with year filter 2023), I receive a clear explanation before execution, saving me time and helping me learn the correct approach.

**US-FA-13**: As an analyst, the system warns me when my date ranges or filters are likely to return empty or extremely large result sets, allowing me to adjust my query before wasting time on execution.

**US-FA-15**: As an analyst, when I query vendor data but accidentally select a customer record type, the system catches this mismatch and explains why this combination won't work, preventing confusion.

#### New Tasks

**T2.13 • Semantic Validation Rules Engine**
- **Purpose:** Validate logical consistency and business rule compliance of query plans
- **Inputs:** SavedSearchPlan, business rules configuration, temporal analysis, entity relationship rules, tenant metadata
- **Output:** Semantic validator with error/warning classifications and explanatory messages
- **Owner:** Backend
- **Dependencies:** T2.7 (Guardrails), T1.3 (Business Glossary)
- **Acceptance Criteria:**
  - Rule categories: Temporal consistency (15+ rules), Entity relationships (10+ rules), Business logic (10+ rules), Performance warnings (8+ rules), Common mistakes (12+ rules)
  - Classification: Errors block execution, Warnings allow proceed with confirmation
  - Each issue includes: Clear message, Explanation why it's problematic, Suggested fix, Examples of correct usage
  - Execution: Runs after Layer 4, adds <50ms to validation pipeline
  - Extensibility: New rules added via YAML configuration without code changes
  - Tenant-specific rules: Support for org-specific business logic

**T2.14 • Business Rules Configuration Repository**
- **Purpose:** Declarative YAML-based rules defining semantic constraints
- **Inputs:** Subject matter expert input, historical error patterns, NetSuite best practices, tenant-specific requirements
- **Output:** Versioned YAML files with 50+ initial semantic rules
- **Owner:** BA + Backend
- **Dependencies:** T2.13 (Rules Engine)
- **Acceptance Criteria:**
  - Rule structure: ID, Type (error/warning), Severity (high/medium/low), Condition (expression), Message template, Suggestion template, Examples (good/bad), Explanation text
  - Categories organized by domain: Temporal, Entities, Financial, Performance, Data quality
  - Version control in Git with change history
  - Validation of rule file syntax on commit
  - Documentation: Each rule explains rationale and when it applies
  - Testing: Unit tests covering all rules with positive/negative cases

**T6.13 • Semantic Warning Dialog**
- **Purpose:** Present semantic issues to users with educational context
- **Inputs:** Semantic validation results from T2.13
- **Output:** Modal dialog with issue cards, suggestions, and proceed/cancel options
- **Owner:** Frontend
- **Dependencies:** T2.13 (Semantic Validator)
- **Acceptance Criteria:**
  - Visual distinction: Errors (red, must fix) vs Warnings (yellow, can proceed)
  - Issue cards show: Icon, Message, Detailed explanation (expandable), Suggestion with example, Good/bad examples if available
  - For warnings only: "I understand, proceed anyway" checkbox required
  - Actions: "Fix Issues" (returns to query input), "Cancel", "Proceed" (warnings only)
  - Accessible: Screen reader friendly, keyboard navigation
  - Mobile optimized: Stacked cards, touch-friendly
  - Analytics: Track which warnings users override most frequently

**T2.X • Semantic Rule Testing Framework**
- **Purpose:** Comprehensive testing of semantic rules to prevent false positives
- **Inputs:** Rule definitions, test case library
- **Output:** Automated test suite with coverage reporting
- **Owner:** QA + Backend
- **Dependencies:** T2.13, T2.14
- **Acceptance Criteria:**
  - Minimum 5 test cases per rule (positive and negative scenarios)
  - Edge case coverage: Boundary conditions, null values, extreme ranges
  - False positive detection: Valid queries must not trigger rules incorrectly
  - Performance testing: Rule evaluation <50ms for complex plans
  - Regression prevention: Historical issues included as test cases
  - CI integration: Tests run on every rule file change

#### Architecture Additions

**Semantic Rule Categories:**

**Category 1: Temporal Consistency (15 rules)**
- Future date check: No dates beyond current date
- Period-year mismatch: Fiscal period must match year filter if both specified
- Date range inversion: End date must be after start date
- Ambiguous relative dates: "last month" near month boundary
- Period boundary issues: Quarter-end, year-end edge cases
- Fiscal vs calendar year confusion
- Period closure status: Can't modify closed periods
- Date range excessive: >5 years triggers warning
- Missing period with posting filter
- Overlapping date filters (redundant or contradictory)

**Category 2: Entity Relationships (10 rules)**
- Record type field mismatch: Vendor fields on customer records
- Incompatible dimensions: Employee + vendor combinations
- Subsidiary-specific fields without subsidiary filter
- Missing required context: Multi-subsidiary orgs need subsidiary
- Entity type compatibility: Customer transaction but vendor filters
- Hierarchical mismatches: Parent-child entity confusion
- Cross-entity queries: Unsupported joins between record types

**Category 3: Business Logic (10 rules)**
- Debit-credit overlap: Same accounts in both filters
- Account type misuse: Expense account in revenue query
- Transaction type constraints: Only certain types have certain fields
- Posting status implications: Unposted transactions limited visibility
- Currency considerations: Multi-currency without currency filter
- Intercompany eliminations: Special handling required
- Department/class/location: Required for certain transaction types

**Category 4: Performance Warnings (8 rules)**
- Large date range: >3 years
- No time filter with posting: Very slow queries
- Wildcard inefficiency: Short search terms (<3 chars)
- Missing indexes: Filters on non-indexed fields
- Result set size: Estimated >100K rows
- Complex aggregations: Multiple groupings + calculations
- Subsidiary "all": Better to query specific subsidiaries

**Category 5: Common Mistakes (12 rules)**
- Empty result predictors: Filters likely yield no results
- First-of-month temporal: "last X" on day 1-3 of period
- Typo detection: Subsidiary/account name fuzzy matching
- Missing critical dimensions: Revenue without subsidiary
- Overly broad queries: No filters at all
- Redundant filters: Multiple filters achieving same result
- Case sensitivity: Name matching issues

**Rule Definition Example:**

```yaml
- id: future_date_check
  category: temporal
  type: error
  severity: high
  condition: |
    any_filter.field == 'trandate' && 
    any_filter.operator == 'after' &&
    parse_date(any_filter.value) > current_date
  message: "Cannot query transactions from future dates ({future_date})"
  suggestion: "Check your date range - did you mean {corrected_date}?"
  explanation: "Transaction dates cannot be in the future. Verify the year and month in your query."
  examples:
    bad: "transactions dated 2026-01-01"
    good: "transactions dated 2024-01-01 to 2024-12-31"
  impact: "This query would return zero results"
  documentation_link: "/docs/date-filters"
```

**Validation Pipeline Integration:**

**Existing Flow:**
1. LLM generates SavedSearchPlan
2. Layer 1: JSON validation
3. Layer 2: Schema validation
4. Layer 3: Operator validation
5. Layer 4: Guardrails validation
6. If all pass → Execute

**Enhanced Flow:**
1. LLM generates SavedSearchPlan
2. Layer 1: JSON validation
3. Layer 2: Schema validation
4. Layer 3: Operator validation
5. Layer 4: Guardrails validation
6. **NEW: Semantic validation** (errors block, warnings prompt)
7. If errors → Return 400 with semantic issues
8. If warnings → Return 200 with plan + warnings (UI prompts user)
9. If all pass → Execute

**Context Enrichment for Rules:**

Rules need context beyond the plan itself:

- **Current date/time:** For relative date evaluation
- **Tenant metadata:** Multi-subsidiary, fiscal calendar, default currency
- **Period status:** Which periods are open/closed
- **User preferences:** Default subsidiary, date format
- **Historical patterns:** This user's typical queries
- **Record schema:** Available fields, relationships
- **Performance stats:** Estimated row counts for filters

**Error Response Format:**

```json
{
  "status": 400,
  "code": "VAL_SEMANTIC",
  "valid": false,
  "errors": [
    {
      "ruleId": "period_year_mismatch",
      "severity": "high",
      "message": "Fiscal period 'Q4 FY2024' is not in year 2023",
      "suggestion": "Remove the year filter or change period to Q4 FY2023",
      "affectedFields": ["period", "year"],
      "examples": {
        "bad": "period='Q4 FY2024' AND year=2023",
        "good": "period='Q4 FY2024'"
      }
    }
  ],
  "warnings": [],
  "suggestions": [
    "Remove the year=2023 filter",
    "Change period to Q4 FY2023"
  ]
}
```

**Warning Response Format:**

```json
{
  "status": 200,
  "plan": { /* SavedSearchPlan */ },
  "warnings": [
    {
      "ruleId": "large_date_range",
      "severity": "medium",
      "message": "Querying 5 years of data may take longer than expected",
      "suggestion": "Consider narrowing to 1-2 years or use async export",
      "explanation": "Large date ranges can take 30+ seconds to execute",
      "impact": "Estimated time: 45 seconds",
      "canProceed": true
    }
  ],
  "requiresConfirmation": true
}
```

**User Experience:**

**Scenario 1: Error - Cannot Proceed**
1. User submits query: "Vendor spend in Q4 2024 for year 2023"
2. Semantic validation catches period-year mismatch
3. Dialog appears with red error icon
4. Message explains the conflict clearly
5. Suggestion offers two fixes
6. Only options: "Fix Query" or "Cancel"
7. User clicks "Fix Query", returns to input with error highlighted

**Scenario 2: Warning - Can Override**
1. User submits query: "All transactions from 2020-2024"
2. Semantic validation flags large date range
3. Dialog appears with yellow warning icon
4. Message explains performance impact
5. Shows estimated execution time: 45-60 seconds
6. Checkbox: "I understand this may take over 60 seconds"
7. Options: "Refine Query" or "Proceed Anyway"
8. User checks box and proceeds
9. Query executes, feedback recorded for rule tuning

**Rule Maintenance Workflow:**

**Adding New Rule:**
1. BA identifies pattern from support tickets or user feedback
2. Creates rule definition in YAML
3. Writes test cases (positive/negative)
4. Submits PR with rule + tests
5. Code review by backend + ML team
6. Automated tests run in CI
7. Merge to main, deployed in next release
8. Monitor false positive rate for 2 weeks
9. Tune rule threshold if needed

**Deprecating Rule:**
1. Analytics show rule has 50%+ false positive rate
2. Investigation reveals business logic changed
3. Rule marked deprecated in YAML
4. Monitoring continues for 30 days
5. If no issues, rule removed
6. Or rule revised and re-enabled

**Benefits Quantified:**

| Metric | Before | After | Improvement |
|--------|--------|-------|-------------|
| Queries with logical errors | 15% | 3% | 80% reduction |
| Empty result queries | 12% | 4% | 67% reduction |
| Wasted API units | 100% | 85% | 15% savings |
| Average query success time | 8.5s | 6.2s | 27% faster |
| User confusion tickets | 20/month | 6/month | 70% reduction |
| Query refinement iterations | 2.3 avg | 1.4 avg | 39% fewer retries |

---

## Recommendation 8: Template Governance Workflow {#recommendation-8}

### Why This Recommendation

Phase 3 implements template storage and reuse (T3.1-T3.5) but lacks organizational governance:

**Current Gaps:**

**Quality Control:**
- Any user can create templates that others will use
- No review process before templates go live
- Bad templates can proliferate across organization
- No quality standards or certification process

**Change Management:**
- Template updates happen without oversight
- No versioning or rollback capability
- Breaking changes impact all users immediately
- No communication of template modifications

**Ownership & Accountability:**
- Unclear who maintains each template
- Orphaned templates when creators leave
- No designated reviewers or approvers
- Responsibility diffusion leads to quality decay

**Lifecycle Management:**
- No deprecation process for outdated templates
- Templates accumulate indefinitely
- No archival of unused templates
- Cluttered template library reduces usability

**Compliance & Audit:**
- Can't trace who approved templates for financial reports
- No record of template change history
- Inability to demonstrate governance for auditors
- Regulatory risk for sensitive queries

### What This Improves

**Organizational Quality:**
- Only vetted, approved templates available organization-wide
- Consistent quality standards enforced
- Best practices captured and shared formally
- Reduced variability in reporting approaches

**Change Control:**
- Controlled rollout of template updates
- Impact assessment before changes
- Rollback capability if issues arise
- User notification of breaking changes

**Clear Accountability:**
- Designated template owners for maintenance
- Approval workflow with named reviewers
- Audit trail of all decisions
- Performance accountability for template quality

**Knowledge Management:**
- Curated library of official templates
- Institutional knowledge captured in templates
- Onboarding accelerated through certified templates
- Best practices socialized organization-wide

**Compliance Readiness:**
- Demonstrable governance for auditors
- Complete change history for regulated reports
- Approval workflows meet SOX requirements
- Risk mitigation through controlled access

**User Experience:**
- Trust in official templates
- Clear distinction: Personal vs Team vs Org templates
- Confidence that templates are maintained
- Reduced clutter from deprecated templates

### Implementation Guidance

#### New User Stories

**US-NS-05**: As an administrator, I can review, approve, and manage templates before they're available to all users, ensuring quality and consistency across the organization through a structured workflow with clear approval gates.

**US-FA-09**: As a power user, when I've created a query that I've used successfully 10+ times, I can propose it as an organization-wide template through a one-click publish flow that includes validation testing and admin review.

**US-FM-03**: As a finance manager, when browsing templates, I can clearly see which are officially approved by the organization versus personal templates, with quality badges and ownership information helping me decide which to trust.

**US-ADMIN-01**: As an administrator, I receive weekly digest reports showing pending template approvals, recently published templates, deprecated templates needing replacement, and templates failing quality checks.

#### New Tasks

**T3.8 • Template Lifecycle State Machine**
- **Purpose:** Define template states and govern transitions through workflow
- **Inputs:** Governance policies, approval requirements, state transition rules
- **Output:** State machine implementation with transition validation and audit logging
- **Owner:** Backend
- **Dependencies:** T3.1 (Template Canonical Format)
- **Acceptance Criteria:**
  - States: Draft (creator working), Pending Review (submitted for approval), Approved (passed review), Active (live for users), Deprecated (visible with warning), Archived (hidden)
  - Transitions require appropriate permissions based on scope
  - Email notifications on state changes to relevant stakeholders
  - Complete audit trail: Who changed state, when, why (reason required)
  - Blocking transitions: Can't activate without approval, can't archive active templates without deprecation period
  - Automatic transitions: Draft auto-archives after 90 days of inactivity

**T3.9 • Template Publishing Workflow**
- **Purpose:** Enable users to propose successful queries as organization templates
- **Inputs:** User's query history (success rate, usage count metrics)
- **Output:** "Publish Template" UI flow, admin approval queue, notification system
- **Owner:** Frontend + Backend
- **Dependencies:** T3.8 (Lifecycle Management)
- **Acceptance Criteria:**
  - Eligibility criteria: Query used 10+ times with 90%+ success rate by user
  - One-click publish from query history with pre-populated metadata
  - Required fields: Template name, Description, Category, Intended audience, Test cases (optional)
  - Auto-populate: Structure, slots, average execution time, success rate, column definitions
  - Preview mode: Submit as draft for testing before formal submission
  - Submission creates review ticket in admin queue
  - Automated quality checks run immediately (T3.6 validation)
  - Email to reviewers: "New template pending: [Name] by [User]"
  - Feedback loop: If rejected, user gets explanation and can resubmit

**T3.10 • Template Version Control System**
- **Purpose:** Track template changes over time with rollback capability
- **Inputs:** Template modifications, changelogs, version metadata
- **Output:** Git-like version history with comparison and rollback features
- **Owner:** Backend
- **Dependencies:** T3.1, T3.8
- **Acceptance Criteria:**
  - Semantic versioning: Major.Minor.Patch (e.g., 2.1.3)
  - Major bump: Breaking changes (slot modifications, structure changes)
  - Minor bump: Enhancements (additional columns, improved filters)
  - Patch bump: Bug fixes (operator corrections, minor adjustments)
  - Changelog required for all versions: What changed, why, breaking vs non-breaking
  - Version comparison: Side-by-side diff view showing structural changes
  - Rollback capability: One-click revert to previous version (creates new version)
  - Deprecation of old versions: Only latest 3 major versions kept active
  - Migration path: Provide upgrade guidance for users of deprecated versions

**T3.11 • Template Ownership & Permission Model**
- **Purpose:** Define access control for template visibility, usage, and modification
- **Inputs:** RBAC system, organizational hierarchy (departments/teams), user roles
- **Output:** Permission matrix with enforcement at API and UI levels
- **Owner:** Backend
- **Dependencies:** T3.8 (Lifecycle)
- **Acceptance Criteria:**
  - Scope types: Personal (creator only), Team (specific department/team), Organization (all users with appropriate role)
  - Permissions: View (see template exists), Use (execute queries with template), Edit (modify template), Approve (review and activate), Transfer (change ownership)
  - Role mapping: End User (view/use), Power User (view/use/create personal/team), Admin (all permissions)
  - Transfer workflow: Owner can designate successor, admin can force transfer if owner leaves
  - Inheritance: Team templates visible to team members, org templates to everyone
  - Delegation: Owners can designate co-maintainers

**T3.X • Template Review Queue Dashboard**
- **Purpose:** Centralized interface for admins to manage template approvals
- **Inputs:** Pending templates, quality metrics, user feedback, usage stats
- **Output:** Admin dashboard with review workflow, bulk actions, reporting
- **Owner:** Frontend
- **Dependencies:** T3.8, T3.9, T3.6 (Quality Scoring)
- **Acceptance Criteria:**
  - Queue sorted by priority: P0 (>100 potential users), P1 (active need), P2 (nice-to-have)
  - Template cards show: Name, Creator, Submission date, Quality score, Validation results, Estimated impact, Category
  - Actions per template: Test with sample data, View structure, Compare to similar templates, Approve, Reject with feedback, Request changes
  - Bulk actions: Approve multiple, Batch reject, Assign to reviewer
  - Filters: By category, by submitter, by date range, by quality score
  - Search across template names and descriptions
  - Weekly digest: Summary email of queue status

#### Architecture Additions

**Template Lifecycle States:**

**State: Draft**
- **Description:** Creator is developing/testing template
- **Visibility:** Creator only
- **Actions:** Edit, Delete, Test, Submit for Review
- **Auto-transitions:** Archive after 90 days inactive
- **Notifications:** None

**State: Pending Review**
- **Description:** Submitted awaiting admin approval
- **Visibility:** Creator + Admins
- **Actions:** Approve, Reject, Request Changes, Test
- **SLA:** Review within 5 business days
- **Notifications:** Reviewers on submission, creator on status change

**State: Approved**
- **Description:** Passed review but not yet activated
- **Visibility:** Creator + Admins
- **Actions:** Activate, Edit (returns to Pending), Reject
- **Purpose:** Allows scheduling of template release
- **Notifications:** Creator on approval

**State: Active**
- **Description:** Live and available for use
- **Visibility:** Based on scope (personal/team/org)
- **Actions:** Use, Edit (creates new version), Deprecate
- **Quality:** Monitored continuously via T3.6
- **Notifications:** Users on major version updates

**State: Deprecated**
- **Description:** Still usable but not recommended
- **Visibility:** Same as Active but with warning badge
- **Actions:** Use (with warning), Archive, Reactivate
- **Display:** "Deprecated - Use [Alternative] instead"
- **Grace Period:** 30 days before auto-archive
- **Notifications:** Users on deprecation with migration path

**State: Archived**
- **Description:** Hidden from search, preserved for audit
- **Visibility:** Admins only in archive view
- **Actions:** View (read-only), Restore, Permanently Delete (after retention)
- **Retention:** 2 years then eligible for deletion
- **Notifications:** None

**Permission Matrix:**

| Role | Personal | Team | Organization |
|------|----------|------|--------------|
| End User | View own,**Enhanced Reliability:**
- Continue operating during NetSuite API slowdowns or outages
- Stale cache (up to 24 hours old) better than no service
- Graceful degradation instead of hard failures
- 99.9% availability for metadata lookups (vs 95% without caching)

**Scalability Unlocked:**
- Support 100+ concurrent users without NetSuite throttling concerns
- Linear scaling via Redis cluster (vs NetSuite API bottleneck)
- Predictable performance regardless of NetSuite load
- Room for 10x user growth with same infrastructure

**Operational Benefits:**
- Reduced alert noise from transient NetSuite issues
- Lower infrastructure costs through API unit savings
- Better capacity planning with decoupled dependencies
- Improved observability through cache metrics

### Implementation Guidance

#### New User Stories

**US-SYS-03**: As a system, I maintain fresh metadata caches with intelligent TTL policies and background refresh, minimizing NetSuite API calls while ensuring users always see current options for subsidiaries, periods, accounts, and other reference data.

**US-FA-11**: As an analyst, when I start typing a query that mentions subsidiaries or periods, the disambiguation options appear instantly because the system has pre-fetched relevant metadata based on my query context.

**US-OPS-05**: As operations, I can monitor cache effectiveness through dashboards showing hit rates, miss patterns, refresh cycles, and API call reduction metrics, with alerts when cache health degrades.

#### New Tasks

**T1.9 • Metadata Caching Service**
- **Purpose:** Intelligent multi-tier caching of NetSuite reference data with TTL management
- **Inputs:** NetSuite REST API endpoints, cache policy configuration (TTLs, refresh strategies)
- **Output:** Redis cluster with hierarchical caching and automatic refresh
- **Owner:** Backend
- **Dependencies:** T0.2 (Environments Provisioned)
- **Acceptance Criteria:**
  - Cache types with differentiated TTLs: Subsidiaries (24h), Periods (1h), Accounts (6h), Vendors/Customers (2h), Employees (4h), Departments/Locations/Classes (12h), Custom Fields (15m supplement to daily crawl)
  - Lazy loading: Fetch on first miss, then cache for subsequent requests
  - Background refresh: When cache item reaches 50% of TTL, refresh asynchronously without blocking user request
  - Stale cache fallback: Keep last-known-good value for 7 days as emergency fallback during NetSuite outages
  - Per-tenant isolation: Separate cache namespaces to prevent cross-tenant data leakage
  - Cache warming on tenant onboarding: Pre-populate critical metadata
  - Hit rate target: >85% within 1 week of production deployment
  - Cache invalidation API for manual refresh when needed

**T1.10 • Smart Prefetch Service**
- **Purpose:** Predictively load metadata likely needed for disambiguation based on query analysis
- **Inputs:** Natural language query text, historical query patterns, entity extraction
- **Output:** Non-blocking background prefetch jobs triggered on query submission
- **Owner:** Backend
- **Dependencies:** T1.9 (Metadata Caching), T2.8 (Disambiguation Detector)
- **Acceptance Criteria:**
  - Entity detection: If query mentions "ACME" or "subsidiary", prefetch subsidiary list
  - Temporal detection: If query mentions dates/quarters/years, prefetch accounting periods
  - Financial keywords: If "revenue"/"expense"/"account" detected, prefetch chart of accounts
  - Vendor/customer detection: Prefetch top 100 vendors or customers based on keywords
  - Prefetch completes in <200ms, doesn't block query planning
  - 50% reduction in disambiguation dialog latency (measured pre/post implementation)
  - Prefetch accuracy: 70%+ of prefetched data actually used in disambiguation
  - Background execution: Does not delay query planning if prefetch slow/fails

**T1.11 • Cache Invalidation Webhooks**
- **Purpose:** Real-time cache invalidation when NetSuite data changes
- **Inputs:** NetSuite workflow/SuiteScript notifications via webhook
- **Output:** Webhook endpoint, signature validation, targeted invalidation
- **Owner:** Backend
- **Dependencies:** T1.9 (Metadata Caching Service)
- **Acceptance Criteria:**
  - Webhook endpoint accepting POST requests from NetSuite workflows
  - HMAC signature validation for security (prevent unauthorized invalidation)
  - Granular invalidation: Only clear affected cache entries, not entire cache
  - Event types supported: Subsidiary created/updated, Account created/modified, Period closed, Custom field added
  - Idempotency: Handle duplicate webhook deliveries gracefully
  - Fallback: If webhooks fail, TTL-based expiry ensures eventual consistency
  - Replay protection: Track webhook IDs to prevent replay attacks
  - Monitoring: Track webhook reception rate, invalidation events, signature failures

**T1.X • Cache Monitoring Dashboard**
- **Purpose:** Visibility into cache performance and health
- **Inputs:** Cache metrics from Redis, application telemetry
- **Output:** Grafana dashboard with key performance indicators
- **Owner:** DevOps
- **Dependencies:** T1.9, T7.3 (Observability)
- **Acceptance Criteria:**
  - Overall cache hit rate (target: >85%)
  - Hit rate by cache type (identify which caches underperforming)
  - Cache size and growth trends
  - Eviction rate (should be low with proper TTL tuning)
  - Average latency: cache hit vs cache miss
  - Background refresh success rate
  - Stale fallback usage (indicator of NetSuite availability issues)
  - Top cache keys by access frequency
  - Alert: Hit rate drops below 80% for 15 minutes
  - Alert: Cache service unavailable

#### Architecture Additions

**Cache Type Definitions:**

Each cache type optimized for its data characteristics and access patterns:

| Cache Type | TTL | Refresh Strategy | Size Estimate | Volatility |
|------------|-----|------------------|---------------|------------|
| Subsidiaries | 24h | Background at 12h | 10-100 entries | Very Low |
| Accounting Periods | 1h | Background at 30m | 50-200 entries | Low (monthly) |
| Chart of Accounts | 6h | Background at 3h | 500-5000 entries | Low |
| Vendors (top 100) | 2h | Background at 1h | 100 entries/tenant | Medium |
| Customers (top 100) | 2h | Background at 1h | 100 entries/tenant | Medium |
| Employees | 4h | Background at 2h | 50-500 entries | Medium |
| Departments | 12h | Background at 6h | 20-100 entries | Low |
| Locations | 12h | Background at 6h | 10-50 entries | Low |
| Classes | 12h | Background at 6h | 10-100 entries | Low |
| Custom Fields | 15m | Background at 7m | 100-500 entries | High (supplements T1.5) |

**Caching Architecture Layers:**

**Layer 1: Application-Level Cache (In-Memory)**
- Ultra-fast lookup for hot data (<1ms)
- LRU eviction, max 1000 entries
- 5-minute TTL, refresh from Layer 2
- Use for: Currently authenticated user's preferences, recent query subsidiaries

**Layer 2: Distributed Cache (Redis)**
- Primary caching layer (5-10ms latency)
- Persistent across application restarts
- Shared across all application instances
- TTL management and background refresh
- Use for: All NetSuite metadata

**Layer 3: Stale Cache (Redis)**
- Emergency fallback during outages
- 7-day retention of last-known-good values
- Only accessed when fresh fetch fails
- Marked with staleness indicator

**Cache Key Structure:**
```
ns:cache:{cache_type}:{tenant_id}:{filter_hash}

Examples:
ns:cache:subsidiaries:acme_corp:all
ns:cache:accounts:acme_corp:type=expense
ns:cache:vendors:acme_corp:limit=100&active=true
```

**Cache Workflow:**

**On Query Planning (Layer 2 Validation):**
1. Need to validate field "custbody_department" exists
2. Check Layer 1 (in-memory): Miss
3. Check Layer 2 (Redis): Hit! Return in 8ms
4. Check TTL: 30% remaining (within 50% threshold)
5. Background job: Trigger async refresh from NetSuite
6. Continue validation with cached data (no waiting)

**On Disambiguation:**
1. Query mentions "subsidiary" → need subsidiary list
2. Prefetch service already loaded it (if working) → instant
3. If not prefetched: Check Redis cache
4. If hit: Return immediately
5. If miss: Fetch from NetSuite, cache for 24h, return
6. Show in UI with <100ms total latency

**Cache Invalidation Flow:**
1. NetSuite admin creates new subsidiary "ACME Canada"
2. SuiteScript workflow triggers webhook to cache service
3. Webhook received, signature validated
4. Extract: event=subsidiary_created, tenant=acme_corp
5. Invalidate: ns:cache:subsidiaries:acme_corp:*
6. Next query triggers fresh fetch including new subsidiary
7. Alternative: If webhook not configured, TTL expiry ensures consistency within 24h

**Prefetch Intelligence:**

**Query Pattern Analysis:**
```
Query: "Show me vendor spend in ACME US for Q4 2024"

Entity Extraction:
- "ACME US" → subsidiaries likely needed
- "Q4 2024" → periods likely needed
- "vendor" → vendors likely needed

Prefetch Actions (parallel, non-blocking):
1. Load subsidiaries (if not cached)
2. Load accounting periods (if not cached)
3. Load top 100 vendors for this tenant (if not cached)

Latency: 150ms in background while LLM planning
Result: By time disambiguation needed, all options pre-loaded
```

**Cache Warming Strategy:**

**On Tenant Onboarding:**
1. Immediately fetch and cache: Subsidiaries, Accounting Periods, Top-level Accounts
2. Background jobs over next hour: Full chart of accounts, Top 100 vendors/customers, Departments, Locations
3. Result: First user query feels instant

**On Application Startup:**
1. Reconnect to Redis
2. Verify cache accessibility
3. Pre-warm: Last 7 days' most-queried metadata
4. Mark as ready for traffic

**Monitoring & Alerting:**

**Key Metrics:**
- **Cache Hit Rate:** (hits / (hits + misses)) * 100
  - Target: >85% overall
  - Alert if <80% for 15 minutes
  
- **Cache Latency:**
  - P50 cache hit: <5ms
  - P95 cache hit: <15ms
  - P95 cache miss: <250ms (includes NetSuite fetch)
  
- **Background Refresh:**
  - Success rate: >98%
  - Latency: <200ms
  - Alert if success rate <95%
  
- **API Call Reduction:**
  - Baseline: 100% (all calls to NetSuite)
  - Target: 15-20% (80-85% reduction)
  - Track daily: API calls / queries ratio

**Cost Analysis:**

**Infrastructure Costs:**
- Redis cluster (HA): $200/month
- Additional egress: $50/month
- Total: $250/month

**API Cost Savings:**
- Baseline: 20 API units/query * 10,000 queries/month = 200,000 units
- With caching (85% hit rate): 3 API units/query = 30,000 units
- Savings: 170,000 units/month
- At $0.02/unit: $3,400/month saved

**ROI: $3,150/month net savings (1,260% ROI)**

---

## Recommendation 5: User Onboarding & Query Suggestion Engine {#recommendation-5}

### Why This Recommendation

The system assumes users intuitively know how to phrase natural language queries effectively. This creates significant adoption barriers:

**Discovery Problem:**
- Users don't know what the system can do
- Unclear which metrics, dimensions, and filters are available
- No visibility into successful query patterns
- Trial-and-error wastes time and creates frustration

**Learning Curve:**
- First-time users stare at blank query box unsure what to type
- No guidance on query formulation best practices
- Users don't know when to be specific vs general
- Ambiguity in how to phrase temporal references

**Feature Underutilization:**
- Advanced capabilities (composition, templates) remain undiscovered
- Users stick to simple queries even when complex needs exist
- Export options, feedback mechanisms not noticed
- Full system value not realized

**Support Burden:**
- Repetitive questions: "What can I ask?", "How do I phrase this?"
- Training sessions needed for basic usage
- Documentation insufficient for self-service
- Slow time-to-productivity for new users

**Competitive Context:**
- Modern interfaces (ChatGPT, Google) set expectations for suggestions
- Users expect autocomplete, examples, guided experiences
- Lack of guidance feels dated and unfriendly
- First impression determines adoption success

### What This Improves

**Accelerated Adoption:**
- First successful query within 2 minutes (vs 15+ minutes without guidance)
- 70%+ of new users complete onboarding tutorial
- Time-to-productivity: 1 day vs 1 week
- Reduced abandonment during initial exploration

**Reduced Support Load:**
- 60% reduction in "how do I use this?" tickets
- Self-service success rate increases from 40% to 80%
- Documentation accessed more effectively via in-app examples
- Training time reduced from 1 hour to 15 minutes

**Improved Query Quality:**
- Better-formed questions lead to better results
- Fewer validation errors from malformed queries
- Higher success rate on first attempt (60% → 85%)
- Reduced disambiguation iterations

**Feature Discovery:**
- Users discover exports, feedback, templates organically
- Composition feature adoption 3x higher with guided experience
- Template reuse increases as users understand benefits
- Advanced features utilized within first week vs first month

**User Satisfaction:**
- Confidence from guided experience
- Reduced frustration from guessing
- Perception of intelligent, helpful system
- Positive first impression drives continued usage

### Implementation Guidance

#### New User Stories

**US-FA-07**: As a new analyst, I receive contextual suggestions and examples while typing my query, helping me formulate effective questions quickly so I can be productive on day one without extensive training.

**US-FA-08**: As an analyst, when I begin typing, I see relevant suggestions from past successful queries, golden examples, and business glossary terms, making it faster to construct complex questions without memorizing syntax.

**US-FA-14**: As a first-time user, I experience an interactive tutorial that walks me through all key features (query, disambiguate, results, export, feedback) using demo data, so I feel confident using the system on real data.

**US-PM-01**: As a power user, I can save my frequently-used queries as personal quick-access shortcuts and share successful query patterns with my team.

#### New Tasks

**T6.9 • Interactive Query Builder**
- **Purpose:** Guided wizard-style query construction for users who prefer structured input
- **Inputs:** Available record types, common dimensions from business glossary, golden question templates
- **Output:** Step-by-step form that generates natural language query with live preview
- **Owner:** Frontend
- **Dependencies:** T1.3 (Business Glossary), T0.6 (Golden Questions)
- **Acceptance Criteria:**
  - 3-step wizard: Step 1 (What: select metrics/report type), Step 2 (Where: filters like subsidiary/account/vendor), Step 3 (When: date range or period)
  - Live preview shows natural language query being constructed: "Show me [revenue] for [ACME US] in [Q4 2024]"
  - Toggle between wizard mode and freeform text input
  - Wizard remembers last-used selections for faster repeat queries
  - Save constructed queries as personal templates
  - Pre-populate wizard from golden question examples
  - Accessible via "Guided Mode" button next to query input

**T6.10 • Autocomplete & Suggestion Service**
- **Purpose:** Real-time query suggestions as user types
- **Inputs:** User's partial query text, historical successful queries, golden questions, business glossary, existing templates
- **Output:** Dropdown with top 5 most relevant suggestions with source attribution
- **Owner:** Backend + Frontend
- **Dependencies:** T1.3 (Business Glossary), T0.6 (Golden Questions)
- **Acceptance Criteria:**
  - Activate after 3 characters typed
  - Return suggestions within <100ms (not blocking typing)
  - Multiple sources weighted: User's own history (1.2x), Golden questions (1.0x), Templates (0.9x), Glossary (0.7x)
  - Highlight matching portions of suggestions
  - Show source badge: "From your history", "Example query", "Template", "Business term"
  - Display estimated execution time if known
  - Keyboard navigation: Arrow keys to select, Enter to accept, Esc to close
  - Mobile-friendly: Touch-optimized suggestion cards

**T6.11 • Onboarding Tutorial Flow**
- **Purpose:** First-run interactive experience introducing key features with demo data
- **Inputs:** Tutorial script, sample queries, synthetic demo results
- **Output:** 5-step guided tutorial with progress tracking and completion rewards
- **Owner:** Frontend
- **Dependencies:** T6.1 (Query Console), T6.2 (Disambiguation Dialogs), T6.3 (Results Table)
- **Acceptance Criteria:**
  - 5 tutorial steps: (1) Welcome & overview, (2) Ask a question (with example), (3) Clarify details (simulated disambiguation), (4) Explore results (with sample data), (5) Export & feedback
  - Skip button at any point with resume capability
  - Demo mode: Uses synthetic data, no actual NetSuite calls
  - Progress indicator: "Step 2 of 5"
  - Completion tracked per user in database
  - Completion badge/reward: "Tutorial Complete" to encourage engagement
  - Estimated time: 5-7 minutes
  - Tooltip hints overlay actual UI elements
  - Can replay tutorial from user preferences

**T6.12 • Query Examples Library**
- **Purpose:** Searchable, categorized catalog of example queries with descriptions
- **Inputs:** Golden questions organized by use case, admin-curated examples
- **Output:** Collapsible sidebar panel with categorized examples and search
- **Owner:** Frontend
- **Dependencies:** T0.6 (Golden Questions)
- **Acceptance Criteria:**
  - Categories: Vendor Analysis (10+ examples), Revenue Reporting (10+ examples), Expense Analysis (8+ examples), Financial Analysis (6+ examples), Custom Reports (5+ examples)
  - Each example includes: Query text, plain-English description, expected result summary, difficulty level (basic/intermediate/advanced)
  - Click example to insert into query input box
  - Search functionality across all examples
  - Admin interface to add/edit/remove examples
  - Examples tagged with: record type, key dimensions, complexity level
  - Filter by difficulty level for progressive learning
  - "Try this example" button for one-click execution

**T6.X • Personal Query History & Favorites**
- **Purpose:** Save and quickly access frequently-used queries
- **Inputs:** User's executed queries, manual favorites
- **Output:** History panel with search, favorites, and quick-rerun
- **Owner:** Backend + Frontend
- **Dependencies:** T6.1
- **Acceptance Criteria:**
  - Store last 50 queries per user
  - Star favorite queries for quick access
  - Search across query history
  - One-click rerun with same parameters
  - Edit and rerun with modifications
  - Name saved queries for easy identification
  - Share query with team members (copy link)
  - Export query history as CSV for personal records

#### Architecture Additions

**Suggestion Engine Components:**

**1. Multi-Source Suggestion Aggregator:**
- Queries four sources in parallel
- Merges results with weighted scoring
- De-duplicates similar suggestions
- Returns top 5 ranked by relevance

**2. Source Prioritization:**
- **User History (Weight: 1.2):** User's own past successful queries most relevant
- **Golden Questions (Weight: 1.0):** Curated examples provide quality baseline
- **Templates (Weight: 0.9):** Proven patterns but may be less contextually relevant
- **Glossary Terms (Weight: 0.7):** Educational but may not be complete queries

**3. Relevance Scoring:**
- Prefix match: Highest score (user typing "revenue", "revenue by month" ranks top)
- Contains match: Medium score (user typing "revenue", "top vendors by revenue" included)
- Fuzzy match: Lower score (allows for typos and variations)
- Levenshtein distance for similarity calculation
- Recency boost for history: Recent queries ranked higher

**4. Contextual Enhancement:**
- Learn per-user preferences: If user frequently queries specific subsidiary, boost those suggestions
- Time-based context: "last month" suggestions vary by current date
- Role-based filtering: Show examples relevant to user's access level

**Onboarding Tutorial Architecture:**

**Tutorial State Machine:**
- States: Not started, In progress (step N), Completed, Skipped
- Persistent in user_preferences table
- Resume capability from last step
- Metrics tracked: Start rate, completion rate, average time, skip rate by step

**Demo Data Generation:**
- Synthetic NetSuite-like data for tutorial
- No actual API calls during tutorial
- Realistic but clearly marked as "Demo"
- Edge cases included to demonstrate features

**Tutorial Steps Detail:**

**Step 1 - Welcome:**
- Video or animation showing system overview
- Key benefits highlighted
- "Let's get started" CTA

**Step 2 - Query Input:**
- Example query pre-filled: "Show me vendor spend by category in Q4 2024"
- Explanation of query structure
- User clicks "Submit" to proceed
- Shows planning in progress

**Step 3 - Disambiguation:**
- Simulated 409 response with multiple subsidiary options
- Explanation of why clarification needed
- User selects option and continues
- Demonstrates preference saving

**Step 4 - Results Viewing:**
- Synthetic results displayed in virtualized table
- Highlights: Sorting, filtering, column reordering
- Status bar showing row count
- Encourages exploration

**Step 5 - Export & Feedback:**
- Shows export dialog with tier explanation
- Demonstrates feedback widget
- Explains template transparency
- Congratulations on completion

**Query Examples Organization:**

**Category Structure:**
```
Vendor Analysis/
  ├── Basic/
  │   ├── "Total spend by vendor"
  │   ├── "Top 10 vendors"
  │   └── "Vendor payment terms"
  ├── Intermediate/
  │   ├── "Vendor spend trend over 6 months"
  │   ├── "Vendors with outstanding balances >30 days"
  │   └── "New vendors added this year"
  └── Advanced/
      ├── "Vendor concentration risk analysis"
      └── "Vendor spend vs budget variance"

Revenue Reporting/
  ├── Basic/
  ├── Intermediate/
  └── Advanced/

[Similar structure for other categories]
```

**Example Entry Schema:**
- ID, Category, Subcategory (difficulty)
- Query text
- Description (plain English explanation)
- Expected result summary
- Record type, Key dimensions used
- Tags for search
- Usage count (popularity indicator)
- Created by, Last updated

**Suggestion API Endpoint:**
```
GET /api/v1/suggestions?partial={text}&userId={id}

Response:
{
  "suggestions": [
    {
      "query": "Revenue by month for 2024",
      "source": "golden",
      "category": "Revenue Reporting",
      "confidence": 0.95,
      "description": "Monthly revenue trend analysis",
      "estimatedTime": 2400,
      "usageCount": 247
    },
    {
      "query": "Revenue by subsidiary for Q4",
      "source": "history",
      "confidence": 0.88,
      "lastUsed": "2024-12-15T10:30:00Z",
      "usageCount": 12
    },
    // ... 3 more suggestions
  ],
  "latency": 85
}
```

**User Experience Flow:**

**New User First Visit:**
1. Lands on query console, sees "Start Tutorial" prominent button
2. Clicks tutorial, begins step 1
3. Completes 5-step flow in 6 minutes
4. Returns to console, now familiar with features
5. Clicks "Examples" sidebar, explores categories
6. Finds "Top 10 vendors" example, clicks to insert
7. Submits query, successfully gets results
8. Adds query to favorites for future use

**Returning User:**
1. Starts typing "revenue"
2. Sees suggestions dropdown after 3 characters:
   - "Revenue by month for 2024" (golden)
   - "Revenue by subsidiary for Q4" (own history)
   - "Revenue vs expenses" (template)
3. Arrow keys to select history suggestion
4. Enter to accept
5. Query auto-submitted, results appear
6. Total time: 5 seconds vs 30+ seconds typing full query

**Benefits Quantified:**

| Metric | Without Onboarding | With Onboarding | Improvement |
|--------|-------------------|-----------------|-------------|
| Time to first successful query | 15-30 min | 2-5 min | 75% faster |
| Tutorial completion rate | N/A | 70% | Engagement |
| Support tickets (first week) | 15-20 | 3-5 | 80% reduction |
| Query success rate (first 10 queries) | 60% | 85% | 42% increase |
| Feature discovery (exports/feedback) | 30% | 80% | 167% increase |
| User satisfaction score | 3.2/5 | 4.5/5 | 41% increase |

---

## Recommendation 6: Enhanced Observability for LLM Decision Tracking {#recommendation-6}

### Why This Recommendation

T7.3 covers general observability (latency, errors, uptime), but LLM-based systems have unique debugging and quality assurance needs that aren't addressed:

**Black Box Problem:**
- Why did LLM generate this specific plan structure?
- Which RAG artifacts influenced the decision?
- How confident was the model in its output?
- What alternatives were considered but rejected?
- Cannot debug plan quality issues without visibility

**Quality Assurance Gaps:**
- No way to identify which prompt versions work best
- Cannot correlate RAG context quality with plan success
- Difficult to detect prompt injection or jailbreak attempts
- Missing data for continuous improvement cycles

**Compliance & Audit:**
- Regulatory need for explainable AI decisions
- Must demonstrate how customer data queries were generated
- Audit trail requirements for financial reporting queries
- Cannot trace back from result to decision process

**User Trust:**
- Users don't understand how system interpreted their query
- No transparency into template matching decisions
- Difficult to debug when results don't match expectations
- Trust eroded by "magic box" opacity

**Operational Challenges:**
- Debugging production issues requires code instrumentation
- Cannot isolate whether failure is prompt, RAG, or model issue
- No visibility into token consumption for cost optimization
- Difficult to identify ineffective RAG artifacts

### What This Improves

**Deep Debuggability:**
- Trace any query result back to complete decision chain
- Identify exact RAG artifacts that influenced plan
- Compare successful vs failed plans to find patterns
- Isolate root cause: Prompt issue, RAG retrieval, model behavior, or validation

**Transparency for Users:**
- Show how system interpreted natural language query
- Explain which keywords triggered which filters/columns
- Display confidence scores for trust calibration
- Enable users to understand and verify results

**Quality Improvement:**
- Data-driven prompt engineering based on success metrics
- Identify underperforming RAG artifacts for improvement
- Track model performance degradation over time
- A/B test prompt variations with statistical rigor

**Compliance Readiness:**
- Complete audit trail of AI decision-making
- Demonstrate explainability for regulated queries
- Track which RAG context influenced financial reports
- Meet GDPR, SOX, and industry-specific requirements

**Cost Optimization:**
- Identify queries consuming excessive tokens
- Optimize RAG context to reduce token count
- Track cost per query type for budgeting
- Detect inefficient prompt patterns

**Operational Excellence:**
- Alert on anomalous LLM behavior (sudden accuracy drop)
- Monitor RAG corpus health and effectiveness
- Track exemplar usage to identify best patterns
- Proactive identification of quality degradation

### Implementation Guidance

#### New User Stories

**US-NS-04**: As an administrator, I can review the complete reasoning chain behind any query result, including RAG context used, model parameters, validation steps, and execution details, to debug issues and improve system quality.

**US-FA-12**: As an analyst, I can expand a "How was this built?" panel to see how the system interpreted my query, which keywords mapped to which fields, and the confidence level, helping me understand and trust the results.

**US-MLE-02**: As a machine learning engineer, I can analyze aggregated LLM decision logs to identify which RAG artifacts are most effective, which prompt versions perform best, and where the system struggles, enabling data-driven optimization.

**US-COMP-01**: As a compliance officer, I can export complete audit trails showing how AI-generated queries were constructed, including all data sources consulted and decision points, for regulatory reporting.

#### New Tasks

**T7.12 • LLM Decision Logging**
- **Purpose:** Capture comprehensive metadata for every LLM interaction
- **Inputs:** Planning service telemetry, RAG retrieval results, model responses, validation outcomes
- **Output:** Structured logs in time-series database with full decision chain
- **Owner:** MLE
- **Dependencies:** T2.3 (LLM Prompt + Grammar)
- **Acceptance Criteria:**
  - Log structure includes: Query ID correlation, User context, Complete RAG context (fields/glossary/exemplars retrieved with scores), Model parameters (temp, tokens, latency), Raw prompt and response, Parsed plan output, Confidence scores if available, All validation layer results, Execution outcomes if available
  - Storage: Elasticsearch or similar for full-text search and aggregation
  - Retention: 90 days hot, 1 year cold archive, 7 years for audit-flagged queries
  - Indexing: Query ID, User ID, Tenant ID, Model used, Success/failure, Timestamp
  - Performance: Logging adds <5ms latency (async write)
  - Privacy: PII redaction in logs, encryption at rest
  - Correlate with feedback events when users provide ratings

**T7.13 • Explainability UI Panel**
- **Purpose:** User-facing transparency into how query was interpreted and plan generated
- **Inputs:** T7.12 decision logs retrieved by query ID
- **Output:** Expandable accordion panel showing decision breakdown in user-friendly format
- **Owner:** Frontend
- **Dependencies:** T7.12 (LLM Decision Logging), T6.3 (Results Table)
- **Acceptance Criteria:**
  - Collapsed by default, expandable "How was this built?" button
  - Sections: (1) Query interpretation with highlighted keywords, (2) Knowledge used (RAG artifacts), (3) Model details (which model, timing), (4) Performance metrics, (5) Confidence score if available
  - Visual highlighting: Show which query terms mapped to which plan elements
  - RAG context display: Show top 5 fields/exemplars/glossary terms that influenced decision
  - Confidence visualization: Progress bar or percentage
  - Accessible: Screen reader friendly, keyboard navigable
  - Mobile responsive: Stacked layout on small screens
  - Export capability: Users can download explanation as PDF for records

**T7.14 • RAG Context Attribution Analysis**
- **Purpose:** Dashboard showing which RAG artifacts are most effective at improving plan quality
- **Inputs:** T7.12 aggregated logs, validation success rates, user feedback
- **Output:** Dashboard with RAG corpus health metrics and recommendations
- **Owner:** MLE
- **Dependencies:** T7.12 (Decision Logging), T2.4-T2.7 (Validation Layers)
- **Acceptance Criteria:**
  - Per-exemplar metrics: Usage count, Success rate when used, Average influence score, Validation pass rate
  - Per-glossary-term metrics: Retrieval frequency, Contextual relevance rate, Contribution to successful plans
  - Per-custom-field-descriptor metrics: Usage in plans, Alias effectiveness, Business context utilization
  - Identify low-value artifacts: Exemplars with <60% success rate, Glossary terms never contributing to plans, Custom fields never queried
  - Recommendations: "Remove exemplar X (used 50 times, 40% success)", "Enrich glossary term Y (high retrieval, low relevance)"
  - Monthly report generated automatically
  - Alert: If RAG effectiveness drops >10% week-over-week

**T7.X •# NetSuite RAG Reporting Tool - Strategic Recommendations (Complete)
**Analysis Date:** October 05, 2025  
**Document Version:** v3 Analysis - Complete Edition  
**Reviewer:** System Architecture & Implementation Review

---

## Executive Summary

This document provides comprehensive strategic recommendations for the NetSuite RAG Reporting Tool based on analysis of the combined Tasks & Architecture v3 specification. The recommendations focus on risk mitigation, performance optimization, user experience enhancement, and operational excellence.

**Key Recommendation Areas:**
1. Progressive Rollout Strategy with Feature Flags
2. Enhanced LLM Fallback & Model Versioning
3. Proactive Template Quality Scoring
4. Advanced Caching Layer for NetSuite Metadata
5. User Onboarding & Query Suggestion Engine
6. Enhanced Observability for LLM Decision Tracking
7. Semantic Query Validation Layer
8. Template Governance Workflow
9. Performance Testing Integration
10. Multi-Tenant Cost Attribution

---

## Table of Contents

1. [Recommendation 1: Progressive Rollout Strategy](#recommendation-1)
2. [Recommendation 2: Enhanced LLM Fallback](#recommendation-2)
3. [Recommendation 3: Template Quality Scoring](#recommendation-3)
4. [Recommendation 4: Metadata Caching](#recommendation-4)
5. [Recommendation 5: User Onboarding](#recommendation-5)
6. [Recommendation 6: LLM Observability](#recommendation-6)
7. [Recommendation 7: Semantic Validation](#recommendation-7)
8. [Recommendation 8: Template Governance](#recommendation-8)
9. [Recommendation 9: Performance Testing](#recommendation-9)
10. [Recommendation 10: Cost Attribution](#recommendation-10)
11. [Implementation Roadmap](#implementation-roadmap)
12. [Risk Mitigation Summary](#risk-mitigation-summary)
13. [Success Criteria & Measurement](#success-criteria)

---

## Recommendation 1: Progressive Rollout Strategy with Feature Flags {#recommendation-1}

### Why This Recommendation

The current phased approach (0→1→2→4→6→3→5→7/8/9) creates binary release points where entire phases must be fully complete before user access. This increases risk of:
- **Late discovery of integration issues** - Problems only surface when everything is deployed together
- **User feedback arriving too late** - Can't influence core architecture after full build
- **All-or-nothing deployment** - Single phase failure blocks entire release
- **Difficulty isolating issues** - Hard to determine which component caused production problems
- **Infrastructure scaling challenges** - Must provision for full load immediately

### What This Improves

**Risk Mitigation:**
- Incremental exposure limits blast radius of defects to small user cohorts
- Quick rollback of problematic features without full system redeployment
- Isolated testing of individual features in production environment

**Faster Feedback Loop:**
- Early adopters provide input while development continues on other phases
- Real usage patterns inform design decisions before full commitment
- Identify performance bottlenecks with actual load patterns

**Operational Flexibility:**
- Enable/disable features per tenant, role, or user without code changes
- A/B test alternative implementations to optimize user experience
- Gradual infrastructure scaling based on actual adoption rates

**Business Agility:**
- Launch MVP to subset of users while finishing advanced features
- Marketing can coordinate feature announcements with controlled rollout
- Support team can manage new feature training in waves

### Implementation Guidance

#### New User Stories

**US-OPS-01**: As a system administrator, I can enable/disable features per user cohort (by tenant, role, or individual user) to control rollout risk and gather targeted feedback before full release.

**US-OPS-02**: As a product manager, I can view real-time metrics on feature adoption and performance by cohort to make data-driven rollout decisions.

**US-OPS-03**: As a developer, I can deploy new feature code to production in disabled state, then activate via configuration without redeployment.

#### New Tasks

**T0.8 • Feature Flag Infrastructure**
- **Purpose:** Enable runtime feature toggling without redeployment
- **Inputs:** Feature catalog from PRD (Phase 3 template reuse, Phase 5 composition, SSE streaming, async exports, admin enrichment queue)
- **Output:** Feature flag service (LaunchDarkly, Unleash, or custom solution) with admin UI and audit logging
- **Owner:** DevOps
- **Dependencies:** T0.2 (Environments Provisioned)
- **Acceptance Criteria:**
  - Flags configurable per user, tenant, role, or percentage rollout
  - Real-time updates propagate to all instances within 5 seconds
  - Complete audit trail of flag changes with user attribution
  - Default-safe fallback values if service unavailable
  - SDK integration for both backend and frontend
  - Admin UI for non-technical stakeholders to manage rollouts

**T6.8 • Feature Flag UI Integration**
- **Purpose:** Frontend respects feature flags for conditional rendering and behavior
- **Inputs:** Feature flag SDK, feature requirements from phases 3-5
- **Output:** React hooks/services that check flags before rendering features or enabling actions
- **Owner:** Frontend
- **Dependencies:** T0.8 (Feature Flag Infrastructure), T6.1 (Query Console)
- **Acceptance Criteria:**
  - Graceful degradation when features disabled (no broken UI elements)
  - Loading states during flag resolution to prevent flashing
  - No console errors or broken functionality for disabled features
  - Feature gates on: template reuse banner, composition builder, admin enrichment UI
  - User-friendly messaging when attempting to use disabled features

**T7.X • Feature Flag Metrics Dashboard**
- **Purpose:** Track adoption and performance by feature flag cohort
- **Inputs:** Application metrics tagged with active flags
- **Output:** Grafana dashboard showing usage, performance, and errors by feature flag state
- **Owner:** DevOps/PM
- **Dependencies:** T0.8, T7.3 (Observability Dashboards)
- **Acceptance Criteria:**
  - Show adoption rate (% of eligible users actually using feature)
  - Compare performance metrics (latency, errors) between flag cohorts
  - Track conversion metrics (e.g., template reuse rate when enabled vs disabled)
  - Alert on anomalies when feature flag changes

#### Architecture Additions

**API Layer:**
- Middleware that loads user's active feature flags on each request
- Flag context passed through request lifecycle to enable conditional logic
- Fast in-memory caching of flags with background refresh to minimize latency
- Graceful fallback to default values if flag service unreachable

**Frontend:**
- React context provider that loads flags on app initialization
- Custom hooks (useFeatureFlag, useMultipleFlags) for component access
- Automatic re-render when flags change (via SSE or polling)
- Feature gate HOCs for wrapping entire feature modules

**Database Schema:**
- Feature definitions table with metadata (name, description, default state, rollout strategy)
- User feature overrides table for individual assignments
- Feature flag audit log for compliance and debugging

**Rollout Progression Strategy:**

1. **Week 7-8 (UAT Phase):** Core MVP features only
   - Basic query, validation, disambiguation, execution, results display
   - Template reuse: OFF for all users
   - Composition: OFF for all users
   - Streaming: ON for UAT participants only

2. **Week 9:** Enable streaming for 10% production users
   - Monitor: latency P95, error rate, connection stability
   - Decision gate: If P95 < 5s and errors < 1%, expand to 25%

3. **Week 10:** Enable templates for 25% users (power users first)
   - Monitor: template reuse rate, match accuracy, user feedback
   - Decision gate: If reuse > 40% and quality score > 80, expand to 50%

4. **Week 11-12:** Enable composition for 5-10 beta users
   - Monitor: query complexity, execution time, error patterns
   - Gather qualitative feedback through interviews
   - Decision gate: After 2 weeks stable operation, expand to power users

5. **Week 13-14:** Gradual expansion to 100% based on metrics
   - Each expansion waits 48-72 hours to observe impact
   - Rollback protocol: If errors spike >5% or P95 >12s, immediately disable for new cohorts

**Rollback Procedures:**
- Define clear triggers for automatic rollback (error rate, latency thresholds)
- One-click rollback in admin UI with reason logging
- Communicate status to affected users automatically
- Post-rollback analysis protocol before re-enabling

---

## Recommendation 2: Enhanced LLM Fallback & Model Versioning {#recommendation-2}

### Why This Recommendation

The architecture specifies Ollama with Llama 3.1 8B or Mistral 7B but lacks comprehensive resilience and evolution capabilities:

**Current Gaps:**
- **Single point of failure:** If local LLM service fails or becomes slow, entire query planning stops
- **No model evolution path:** As better models emerge (weekly in LLM landscape), no clear upgrade strategy
- **Lack of performance comparison:** No mechanism to test if Mistral performs better than Llama for this use case
- **Model-specific brittleness:** Prompt engineering may be optimized for one model but break with another
- **No graceful degradation:** System cannot continue with reduced capability if LLM unavailable

**Real-World Failure Scenarios:**
- Ollama container crashes or becomes unresponsive (OOM, network issues)
- Model inference slows down under load (GPU contention)
- Model update introduces regression in plan generation quality
- Security patch requires immediate model replacement

### What This Improves

**System Reliability:**
- Continue functioning during LLM service disruptions (99.9% uptime vs 95% without fallback)
- Automatic failover to backup strategies within milliseconds
- Reduced mean time to recovery (MTTR) from minutes to seconds

**Operational Flexibility:**
- Deploy new model versions alongside existing without downtime (blue-green for models)
- A/B test models to find optimal quality/speed/cost balance
- Route simple queries to faster models, complex to more capable models

**Future-Proofing:**
- Adapt to rapidly evolving LLM landscape without architecture changes
- Easy integration of cloud LLM APIs if local proves insufficient
- Support for specialized models (e.g., finance-tuned) when available

**Cost Optimization:**
- Route 30-40% of simple queries to rule-based planner (zero LLM cost)
- Use smaller/faster models for straightforward queries
- Reserve expensive models for genuinely complex queries

**Quality Improvement:**
- Compare model performance objectively with production traffic
- Identify which model excels at which query types
- Continuous improvement through model selection optimization

### Implementation Guidance

#### New User Stories

**US-SYS-01**: As a system, when the primary LLM is unavailable or slow (>1s timeout), I automatically fall back to alternative planning strategies to maintain service availability with minimal user-visible degradation.

**US-MLE-01**: As a machine learning engineer, I can deploy and test new LLM models alongside existing ones, comparing performance with real production queries before switching 100% of traffic.

**US-OPS-04**: As operations, when LLM service health degrades, I receive automatic alerts and can view which fallback strategies are being used, with clear dashboards showing impact on planning quality.

#### New Tasks

**T2.11 • Multi-Model Abstraction Layer**
- **Purpose:** Abstract LLM interaction to support multiple models and strategies simultaneously
- **Inputs:** Model registry configuration, health check endpoints, routing policies
- **Output:** Unified planning interface that selects optimal strategy per query
- **Owner:** MLE
- **Dependencies:** T2.3 (LLM Prompt + Grammar)
- **Acceptance Criteria:**
  - Support 2+ LLM models (Llama 3.1 8B, Mistral 7B) with independent health tracking
  - Query complexity heuristics for automatic model selection
  - Latency-based automatic failover (<500ms timeout triggers fallback)
  - Per-model prompt template management with version control
  - Strategy priority ordering (rule-based → fast LLM → capable LLM → template match)
  - Circuit breaker pattern per model to prevent cascade failures
  - Telemetry on strategy usage, success rates, latencies per model

**T2.12 • Rule-Based Fallback Planner**
- **Purpose:** Non-LLM planning for simple, common query patterns
- **Inputs:** Query pattern library derived from top 20-30 golden questions
- **Output:** Deterministic pattern matching and plan generation
- **Owner:** Backend
- **Dependencies:** T0.6 (Golden Questions)
- **Acceptance Criteria:**
  - Handles 20+ common patterns: "revenue by month", "top N vendors", "expenses by department"
  - 100% accuracy on matched patterns (validated against golden question expected outputs)
  - <100ms response time (no LLM latency)
  - Graceful pass-through to LLM if pattern doesn't match
  - Pattern library updatable without code deployment
  - Coverage report showing % of queries handleable by rules

**T2.X • Model Health Monitoring**
- **Purpose:** Continuous health checking and performance tracking per model
- **Inputs:** Model endpoints, expected response SLAs
- **Output:** Health status, automatic circuit breaking, alerting
- **Owner:** MLE/DevOps
- **Dependencies:** T2.11
- **Acceptance Criteria:**
  - Heartbeat checks every 10 seconds per model
  - Latency percentile tracking (P50, P95, P99)
  - Error rate monitoring with automatic circuit breaker (5 failures/60s → open circuit)
  - Half-open state for gradual recovery testing
  - Alert on sustained degradation (P95 > 2s for 5 minutes)
  - Dashboard showing model health status and traffic distribution

**T7.10 • Model Performance Comparison Dashboard**
- **Purpose:** Track accuracy, latency, and cost per model variant
- **Inputs:** Telemetry from all planning strategies
- **Output:** Grafana dashboard with comparative metrics and recommendations
- **Owner:** MLE
- **Dependencies:** T7.3 (Observability Dashboards), T2.11
- **Acceptance Criteria:**
  - Per-model success rates (validation passed)
  - Latency distributions (P50/P95/P99) by model
  - Validation failure breakdown by layer (Layer 1-4) per model
  - Token usage and estimated cost per model
  - Query complexity distribution by model (which handles simple vs complex)
  - A/B test results with statistical significance indicators
  - Weekly digest reports to ML team

#### Architecture Additions

**Planning Service Components:**

1. **Planning Orchestrator:**
   - Receives query and user context
   - Evaluates query complexity (simple/moderate/complex)
   - Selects optimal strategy based on complexity, model health, and routing policies
   - Executes strategies in priority order until success
   - Records telemetry on strategy selection and outcomes

2. **Planning Strategies (Priority Order):**
   - **Priority 1: Rule-Based Planner** - Fastest, zero cost, limited scope (handles ~30% of queries)
   - **Priority 2: Primary LLM** - Llama 3.1 8B, general purpose, balanced speed/quality
   - **Priority 3: Fallback LLM** - Mistral 7B, alternative model for redundancy
   - **Priority 4: Template Matcher** - Existing template reuse as last resort if all LLM paths fail

3. **Strategy Interface:**
   - canHandle(query) → boolean: Check if strategy is applicable and healthy
   - generatePlan(query, context) → SavedSearchPlan: Execute planning
   - healthCheck() → HealthStatus: Current operational status
   - metrics → StrategyMetrics: Success rate, latency, cost

4. **Circuit Breaker Per Model:**
   - **Closed (normal):** All traffic flows to model
   - **Open (failure):** No traffic sent, immediate fail to next strategy
   - **Half-Open (testing):** Single probe request to test recovery
   - Transition: Closed → Open after 5 consecutive failures within 60s
   - Transition: Open → Half-Open after 60s cooldown
   - Transition: Half-Open → Closed after successful probe request

**Model Configuration Management:**

- YAML configuration file with model definitions:
  - Model ID, endpoint, model name, prompt template version
  - Traffic weight for A/B testing (e.g., 70% Llama, 30% Mistral)
  - Timeout, temperature, max_tokens, other hyperparameters
  - Enabled/disabled flag for quick toggles
  - Health check interval and circuit breaker thresholds

- Version control for prompt templates:
  - Store templates in Git with semantic versioning
  - Associate each model with specific prompt template version
  - Track performance by prompt version for regression detection
  - A/B test prompt variations alongside model variants

**Failure Scenarios & Responses:**

| Failure Scenario | Detection | Response | User Impact |
|------------------|-----------|----------|-------------|
| LLM service down | Health check fails 3x | Circuit opens, route to fallback | None if fallback succeeds (<500ms additional latency) |
| LLM slow (>1s) | Request timeout | Abort, try next strategy | +200ms for strategy switch |
| Model quality regression | Validation failure rate >20% | Alert, consider model rollback | Increased disambiguation rate |
| GPU memory exhaustion | OOM errors | Circuit opens, route to rule-based + fallback LLM | Reduced capability for complex queries |
| All strategies fail | Exhausted priority list | Return 503 with retry-after | User sees error, can retry |

**Rollout Plan for Multi-Model:**

1. **Week 3-4 (Development):** Implement abstraction layer and rule-based fallback
2. **Week 5:** Deploy alongside existing LLM with feature flag (off by default)
3. **Week 6:** Enable for 10% users, monitor fallback usage and success rates
4. **Week 7:** If stable (<1% additional errors), expand to 50%
5. **Week 8:** Full rollout to 100%, original single-LLM path deprecated

**Success Metrics:**
- System uptime: 99.9% (vs 95% baseline)
- Fallback activation rate: <5% of queries
- Fallback success rate: >90% when activated
- Planning latency P95: <1.2s (acceptable 20% increase for added reliability)
- Rule-based coverage: 30-40% of queries handled without LLM

---

## Recommendation 3: Proactive Template Quality Scoring {#recommendation-3}

### Why This Recommendation

Phase 3 implements template storage and reuse with rejection penalties (T3.5), but this is purely reactive - relying on users to encounter and report issues. Critical gaps:

**Reactive vs Proactive:**
- Templates accumulate 10-50 uses before quality problems surface through feedback
- Each bad template use wastes user time and NetSuite API units
- Negative user experiences damage trust in template system
- Feedback loop requires conscious user action (many don't bother)

**No Quality Gates:**
- Templates enter active rotation immediately after first success
- No verification that success wasn't coincidental or query-specific
- Structural template issues may not manifest until used with different parameters
- Schema drift can silently break templates without detection

**Limited Visibility:**
- Users can't assess template quality before trusting results
- Admins lack early warning of degrading templates
- No objective metrics to identify best vs worst templates
- Template library can accumulate "dead weight" of unused/poor templates

**Real-World Failure Modes:**
- Template works for Q4 but fails for Q1 (fiscal period edge case)
- Template optimized for 1000 rows becomes extremely slow at 50,000 rows
- NetSuite schema change breaks template but it keeps getting matched
- Template for "ACME US" accidentally captures "ACME UK" data

### What This Improves

**Proactive Quality Assurance:**
- Identify bad templates before first user encounter (zero user impact)
- Continuous validation catches schema drift within 24 hours
- Automatic quarantine prevents bad templates from being served
- Build user confidence through transparent quality indicators

**Higher Match Confidence:**
- Only serve templates with proven quality scores >70/100
- Users see confidence badges and can make informed trust decisions
- Reduce false matches through rigorous validation
- Increase template reuse rate through higher trust (target: 60% → 75%)

**Reduced Support Burden:**
- Fewer "wrong results" complaints due to bad templates
- Clear quality indicators help users self-diagnose issues
- Admin alerts enable proactive fixes before user reports
- Automated quarantine reduces escalations

**Data-Driven Optimization:**
- Identify which template patterns work best (inform new template creation)
- Track quality trends over time to detect degradation
- Compare human-created vs auto-generated templates
- Optimize RAG retrieval by quality scores

**Faster Iteration:**
- Quickly validate template improvements before going live
- A/B test template variations with objective quality metrics
- Retire underperforming templates confidently
- Build institutional knowledge of what makes quality templates

### Implementation Guidance

#### New User Stories

**US-SYS-02**: As a system, I continuously evaluate template quality through automated validation against golden questions and real usage patterns, quarantining low-quality templates before they impact users.

**US-FA-10**: As an analyst, when a template is used for my query, I see clear quality indicators (confidence score, success rate, usage count) so I can decide whether to trust the template or request fresh LLM planning.

**US-NS-06**: As an administrator, I receive weekly reports on template library health, with automatic alerts when templates are quarantined, and a prioritized review queue for templates needing attention.

#### New Tasks

**T3.6 • Template Validation Service**
- **Purpose:** Automatically validate templates against golden questions and usage patterns
- **Inputs:** Template library, golden questions (T0.6), production usage statistics
- **Output:** Quality scores (0-100) per template with component breakdowns
- **Owner:** Backend
- **Dependencies:** T3.3 (Dual-Key Retrieval), T0.6 (Golden Questions)
- **Acceptance Criteria:**
  - Run validation on template creation (immediate scoring before first use)
  - Re-validate weekly for all active templates (detect schema drift)
  - Scoring components: correctness (40%), performance (20%), completeness (20%), stability (20%)
  - Flag templates scoring <70 for review, <60 for automatic quarantine
  - Store validation history to track quality trends over time
  - Integrate with NetSuite sandbox for safe validation (no production impact)
  - Nightly batch job completing within 2-hour window

**T3.7 • Template Confidence Indicators UI**
- **Purpose:** Display quality metrics to users when templates are matched/used
- **Inputs:** T3.6 quality scores, real-time usage statistics, user feedback
- **Output:** UI badges, tooltips, and expandable details
- **Owner:** Frontend
- **Dependencies:** T6.5 (Template Transparency), T3.6
- **Acceptance Criteria:**
  - Badge levels: "High Confidence" (85-100), "Medium" (70-84), "Low" (<70)
  - Tooltip shows: quality score, usage count, success rate, avg execution time, last validated
  - "Force Re-Plan with LLM" button for users who distrust template
  - Visual distinction: green for high, yellow for medium, red for low (with accessibility)
  - Show template age and last modified date
  - Display validation details: which golden questions passed/failed

**T7.11 • Template Quarantine Workflow**
- **Purpose:** Automatically disable low-quality templates with admin review process
- **Inputs:** T3.6 quality scores, real-time error rates, user feedback
- **Output:** Quarantine queue UI, automated notifications, review workflow
- **Owner:** Backend
- **Dependencies:** T3.6 (Validation Service)
- **Acceptance Criteria:**
  - Auto-quarantine triggers: score <60, error rate >20% over 7 days, 3+ consecutive user rejections
  - Email alerts to template owner and admin team within 15 minutes
  - Admin review queue showing: template details, validation failures, usage history, suggested fixes
  - One-click actions: restore (with mandatory changelog), permanently delete, deprecate
  - Templates in quarantine hidden from retrieval but preserved for analysis
  - Quarantine duration: 7 days for review before auto-archive
  - Audit log of all quarantine decisions

**T3.X • Golden Question Expansion for Templates**
- **Purpose:** Create template-specific validation questions
- **Inputs:** Template structure, parameter variations, edge cases
- **Output:** Expanded golden question set with parameter matrices
- **Owner:** BA/QA
- **Dependencies:** T0.6, T3.1
- **Acceptance Criteria:**
  - For each golden question, generate parameter variations (different subsidiaries, periods, accounts)
  - Test boundary conditions: empty results, very large results, edge dates (year-end, period-end)
  - Include regression test cases from past template failures
  - Minimum 5 variations per template for comprehensive validation
  - Update validation set when new edge cases discovered

#### Architecture Additions

**Template Quality Metrics Schema:**

Quality scores are multi-dimensional with weighted components:

1. **Correctness Score (40% weight):**
   - % of golden questions returning expected results
   - Column presence: all expected columns in output
   - Row count accuracy: within 5% of expected
   - Value accuracy: spot-check key values match expectations
   - Calculation: (passed tests / total tests) * 100

2. **Performance Score (20% weight):**
   - Execution time vs baseline for query type
   - Scoring: 100 points at 2s, linear decay to 0 at 10s
   - Penalties for timeout or excessive NetSuite units
   - Comparison to fresh LLM-generated plan performance

3. **Completeness Score (20% weight):**
   - All expected columns present and populated
   - No missing data in critical fields
   - Proper handling of null values
   - Appropriate aggregations applied

4. **Stability Score (20% weight):**
   - Based on last 7 days of production usage
   - Success rate in production (executed without errors)
   - User feedback sentiment (thumbs up/down ratio)
   - Schema compatibility checks (fields still exist)
   - Calculation: (1 - error_rate) * 100

5. **Composite Score:**
   - Weighted average of four components
   - Must pass threshold in ALL components (no single weak area)
   - Trending analysis: improving vs degrading

**Validation Pipeline Process:**

1. **Trigger Events:**
   - New template created
   - Weekly scheduled re-validation
   - NetSuite schema change detected
   - Manual validation request from admin
   - After template modification

2. **Validation Execution:**
   - For each applicable golden question:
     - Fill template slots with question parameters
     - Execute against NetSuite sandbox
     - Compare results with expected output (structure and values)
     - Measure execution time
     - Record pass/fail with details
   - Aggregate results into quality scores
   - Generate validation report with specific failures

3. **Decision Logic:**
   - Score ≥85: "High Confidence" - active and promoted
   - Score 70-84: "Medium Confidence" - active with warning
   - Score 60-69: "Low Confidence" - flagged for review
   - Score <60: Automatic quarantine
   - Error rate >20%: Immediate quarantine regardless of score

4. **Health Indicators:**
   - Recent error rate trend (last 7 days)
   - Performance trend (getting slower/faster)
   - Schema compatibility status (all fields valid)
   - User feedback sentiment trend
   - Usage frequency (popular vs abandoned)

**Quarantine Management:**

**Automatic Quarantine Conditions:**
- Quality score drops below 60
- Error rate exceeds 20% over 7 days
- Three consecutive user rejections (force re-plan)
- Schema validation fails (field no longer exists)
- Critical validation failure (returns completely wrong data)

**Quarantine Process:**
1. Template immediately removed from active retrieval
2. In-flight queries using template complete normally
3. Email sent to template owner and admin team
4. Review ticket created in admin queue
5. Template marked with quarantine status and reason
6. After 7 days without action, auto-archive

**Admin Review Queue Priorities:**
- P0: Templates with >100 historical uses (high impact)
- P1: Templates used in last 7 days (actively needed)
- P2: Templates created by admins (official templates)
- P3: Personal templates with low usage

**Review Actions:**
- **Restore:** Fix issues, update template, re-validate, return to active
- **Deprecate:** Keep visible with warning, don't use for new matches
- **Delete:** Remove permanently, preserve for audit
- **Archive:** Store for historical analysis, hidden from users

**User Experience:**

**During Query (Template Match):**
- Show confidence badge prominently in results banner
- Tooltip with detailed metrics on hover
- "This template has been used 247 times with 94% success rate"
- "Last validated 2 days ago, currently passing all checks"
- Button: "Don't trust this? Generate fresh results"

**Quality Indicator Display:**
- Color-coded badges: Green (85+), Yellow (70-84), Red (<70 - rare, usually quarantined)
- Icon system: Checkmark (all validations passed), Warning (some concerns), X (quarantined)
- Expandable details panel showing validation history
- Link to template details page for transparency

**Force Re-Plan Flow:**
1. User clicks "Generate Fresh Results" button
2. System bypasses template, forces LLM planning
3. New plan executed and results displayed
4. Negative signal recorded against template (contributes to rejection penalties)
5. User can provide optional feedback on why they didn't trust template

**Benefits Summary:**

| Metric | Before (Reactive) | After (Proactive) | Improvement |
|--------|-------------------|-------------------|-------------|
| Template Quality | Unknown until user feedback | Scored 0-100 before use | 100% visibility |
| Bad Template Detection | After 10-50 uses | Before first use | Zero user impact |
| Template Trust | 40% reuse rate | 60%+ reuse rate | 50% increase |
| Support Tickets | 10-15/month template issues | 2-3/month | 70% reduction |
| Schema Drift Detection | Days/weeks | 24 hours | 90% faster |
| Admin Effort | Reactive firefighting | Proactive queue management | Structured workflow |

---

## Recommendation 4: Advanced Caching Layer for NetSuite Metadata {#recommendation-4}

### Why This Recommendation

T1.5 implements daily custom field discovery (2 AM UTC) which is good for custom fields, but significant gaps exist for standard metadata and real-time needs:

**Current State Analysis:**
- Every query validation (Layer 2) potentially hits NetSuite API to verify field existence
- No caching of frequently-accessed reference data (subsidiaries, accounts, periods)
- Disambiguation options fetched from NetSuite each time (200-500ms latency)
- High API consumption for repetitive metadata lookups
- No predictive pre-loading of likely-needed data
- Single point of failure if NetSuite API is slow or unavailable during validation

**Performance Impact:**
- Layer 2 validation: 200-400ms per query (60-80% is NetSuite API latency)
- Disambiguation dialog: 300-600ms to populate dropdown options
- Combined: adds 500ms-1s to user-perceived latency unnecessarily
- API costs: 10-20 units per query just for metadata validation
- Throttling risk: High metadata API usage can trigger NetSuite rate limits

**Scalability Constraints:**
- Cannot support 50+ concurrent users without hitting NetSuite API limits
- Each new user multiplies metadata API calls
- Peak hour traffic creates NetSuite API bottleneck
- No room for growth without fundamental architecture change

**Reliability Issues:**
- Transient NetSuite API issues break all query validation
- No stale-data fallback during outages
- Cache misses create user-visible latency spikes
- Dependency on NetSuite responsiveness for core functionality

### What This Improves

**Dramatic Performance Gains:**
- Layer 2 validation: 200ms → <10ms (95% faster) via cached field lookups
- Disambiguation dialogs: 400ms → 50ms (88% faster) via pre-cached options
- First-query latency: 3-4s → 2-2.5s (30% improvement)
- Subsequent queries: Even faster via warmed cache

**Massive Cost Reduction:**
- NetSuite API consumption: -60% to -80% through cache hits
- Targeting 85%+ cache hit rate within one week of operation
- Estimated savings: $2000-5000/month in NetSuite API units (based on projected 10,000 queries/month)
- ROI: Infrastructure costs ($200/month for Redis) vs API savings

**Improved User Experience:**
- Disambiguation options appear instantly (<100ms)
- No waiting for "Loading subsidiaries..." spinners
- Smoother, more responsive interface
- Perceived quality improvement even if query execution time unchanged

**Enhanced Reliability:**
- Continue operating during NetSuite API slowdowns or outages
- Stale cache (up to 24 hours old) better than