select
    'schema.version' as check_item,
    value_ as check_value
from act_ge_property
where name_ = 'schema.version';

select
    table_name
from information_schema.tables
where table_schema = 'public'
  and table_name in (
      'decision_definition',
      'decision_version',
      'decision_execution_log',
      'process_decision_reference'
  )
order by table_name;

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
