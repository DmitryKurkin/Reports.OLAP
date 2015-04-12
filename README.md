# Reports.OLAP
RadarSoft-based reporting tool
==============================

The current version is 0.4.0.0

Notes for XML authors:

1. Measures can only come from the SAME table. Measures from different tables are NOT supported by Radar-Soft.
 Example (GOOD):
 ```xml
 <Measures>
  	<Measure sourceTable="SOME_TABLE" .../>
	<Measure sourceTable="SOME_TABLE" .../>
 </Measures>
 ```
 Example (BAD):
 ```xml
 <Measures>
	<Measure sourceTable="SOME_TABLE" .../>
	<Measure sourceTable="SOME_OTHER_TABLE" .../>
 </Measures>
 ```

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
 ```xml
 <Measure ... aggregateFunction="stAverage" .../>
 ```

3. Possible (library-recognized) values for the "format" attribute:
  - Standard
  - Currency (DEFAULT)
  - Short Date
  - Short Time
  - Percent

 Example:
 ```xml
 <Measure ... format="Short Date" .../>
 ```

4. In addition to the recognized values, the "format" attribute uses the standard .NET formatting rules (see MSDN: https://msdn.microsoft.com/en-us/library/0c899ak8(v=vs.110).aspx)

 Example:
 ```xml
 <Measure ... format="#,#.00" .../>
 ```

5. Symbol '#' must be used in the "function" attribute in order to substitute the corresponding field name. The "alias" attribute is always required when the "function" attribute is used.

 Example:
 ```xml
 <Field name="SOME_FIELD" function="TRIM(L '0' FROM CHAR(#))" alias="PROCESSED_FIELD" .../>
 ```

6. The rules for the JOIN construct:
  1. The Join can be used when two tables (LEFT and RIGHT) are related as either 1-to-1 or 1-to-N
  2. In order for the Join to succeed, the PK name in the LEFT table must be equal to the FK name in the RIGHT table (e.g. leftTable.SOME_ID = rightTable.SOME_ID)
  3. A 'LEFT OUTER JOIN' is used to join the RIGHT table with the LEFT table
  4. The Join REMOVES the PK of the RIGHT table. The PK of the result table becomes the one of the LEFT table. As a consequence, ALL RELATIONS (FKs) that use the RIGHT table as the PARENT one will be DESTROYED
  5. All FKs of both the LEFT and RIGHT tables get moved to the result one unless this contradicts to rule #4
  6. Field name aliases must be used in the RIGHT table definition if this causes field name collisions in the result table
