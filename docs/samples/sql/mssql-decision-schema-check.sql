select
    'schema.version' as check_item,
    [value_] as check_value
from ACT_GE_PROPERTY
where [name_] = 'schema.version';

select
    TABLE_NAME as table_name
from INFORMATION_SCHEMA.TABLES
where TABLE_TYPE = 'BASE TABLE'
  and TABLE_NAME in (
      'decision_definition',
      'decision_version',
      'decision_execution_log',
      'process_decision_reference'
  )
order by TABLE_NAME;

select
    count(*) as decision_definition_count
from decision_definition;

select
    count(*) as decision_version_count
from decision_version;

select
    count(*) as decision_execution_log_count
from decision_execution_log;

select
    count(*) as process_decision_reference_count
from process_decision_reference;
