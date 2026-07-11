create extension if not exists citext;

create table if not exists decision_definition (
    decision_id uuid primary key,
    decision_key citext not null,
    name citext not null,
    category citext null,
    description text null,
    decision_type citext null,
    source_format citext not null default 'DMN',
    status citext not null default 'Draft',
    draft_version_tag citext null,
    published_version_tag citext null,
    tags jsonb not null default '[]'::jsonb,
    created_by citext null,
    created_time timestamptz not null default now(),
    modified_by citext null,
    modified_time timestamptz not null default now()
);

create unique index if not exists ux_decision_definition_key
    on decision_definition (decision_key);

create table if not exists decision_version (
    version_id uuid primary key,
    decision_id uuid not null references decision_definition (decision_id),
    decision_key citext not null,
    version_number bigint not null,
    version_tag citext not null,
    is_published boolean not null default false,
    is_draft boolean not null default false,
    definition_content text not null,
    checksum citext null,
    publish_comment text null,
    published_by citext null,
    published_time timestamptz null,
    created_by citext null,
    created_time timestamptz not null default now()
);

create unique index if not exists ux_decision_version_key_tag
    on decision_version (decision_key, version_tag);

create unique index if not exists ux_decision_version_key_number
    on decision_version (decision_key, version_number);

create index if not exists ix_decision_version_key_published
    on decision_version (decision_key, is_published, version_number desc);

create table if not exists decision_execution_log (
    execution_id uuid primary key,
    decision_key citext not null,
    requested_version_tag citext null,
    resolved_version_tag citext null,
    binding_mode citext null,
    succeeded boolean not null,
    trace_id citext null,
    executor citext null,
    error_message text null,
    matched_rules jsonb not null default '[]'::jsonb,
    input_snapshot jsonb null,
    output_snapshot jsonb null,
    trace_entries jsonb not null default '[]'::jsonb,
    executed_time timestamptz not null default now(),
    elapsed_ms integer null
);

create index if not exists ix_decision_execution_log_key_time
    on decision_execution_log (decision_key, executed_time desc);

create table if not exists process_decision_reference (
    reference_id uuid primary key,
    deployment_id citext not null,
    deployment_name citext not null,
    process_definition_key citext null,
    process_definition_name citext null,
    tenant_id citext null,
    is_draft boolean not null default false,
    deploy_time timestamptz not null,
    decision_key citext null,
    task_id citext null,
    task_name citext null,
    binding_mode citext null,
    version_tag citext null,
    indexed_time timestamptz not null default now()
);

create index if not exists ix_process_decision_reference_key
    on process_decision_reference (decision_key, is_draft, deploy_time desc);

create index if not exists ix_process_decision_reference_deployment
    on process_decision_reference (deployment_id);
