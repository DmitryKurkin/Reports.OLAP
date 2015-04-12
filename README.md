# Reports.OLAP
RadarSoft-based reporting tool

The current version is 0.4.0.0

Notes for XML authors:
1. Measures can only come from the SAME table. Measures from different tables are NOT supported by Radar-Soft.

Example (GOOD):
  <Measures>
		<Measure sourceTable="SOME_TABLE" .../>
		<Measure sourceTable="SOME_TABLE" .../>
	</Measures>

Example (BAD):
  <Measures>
		<Measure sourceTable="SOME_TABLE" .../>
		<Measure sourceTable="SOME_OTHER_TABLE" .../>
	</Measures>

2. Possible values for the "aggregateFunction" attribute:
  - stInherited
  - stCustomAggregated
  - stCalculated
  - stSum (DEFAULT)
  - stCount
  - stAverage
  - stMin
  - stMax
  - stVariance
  - stVarianceB
  - stStdDev
  - stStdDevB
  - stMedian
  - stDistinctCount

Example:
<Measure ... aggregateFunction="stAverage" .../>

3. Possible (library-recognized) values for the "format" attribute:
  - Standard
  - Currency (DEFAULT)
  - Short Date
  - Short Time
  - Percent

Example:
<Measure ... format="Short Date" .../>

4. In addition to the recognized values, the "format" attribute uses the standard .NET formatting rules (see MSDN: https://msdn.microsoft.com/en-us/library/0c899ak8(v=vs.110).aspx)

Example:
<Measure ... format="#,#.00" .../>

5. Symbol '#' must be used in the "function" attribute in order to substitute the corresponding field name. The "alias" attribute is always required when the "function" attribute is used.

Example:
<Field name="SOME_FIELD" function="TRIM(L '0' FROM CHAR(#))" alias="PROCESSED_FIELD" .../>

6. The rules for the JOIN construct:
  a. The Join can be used when two tables (LEFT and RIGHT) are related as either 1-to-1 or 1-to-N
  b. In order for the Join to succeed, the PK name in the LEFT table must be equal to the FK name in the RIGHT table (e.g. leftTable.SOME_ID = rightTable.SOME_ID)
  c. A 'LEFT OUTER JOIN' is used to join the RIGHT table with the LEFT table
  d. The Join REMOVES the PK of the RIGHT table. The PK of the result table becomes the one of the LEFT table. As a consequence, ALL RELATIONS (FKs) that use the RIGHT table as the PARENT one will be DESTROYED
  e. All FKs of both the LEFT and RIGHT tables get moved to the result one unless this contradicts to rule 'd'
  f. Field name aliases must be used in the RIGHT table definition if this causes field name collisions in the result table
