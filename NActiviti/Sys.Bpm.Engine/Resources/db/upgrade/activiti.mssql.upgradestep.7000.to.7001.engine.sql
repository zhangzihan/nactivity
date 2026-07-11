if not exists (
    select 1
    from sys.objects
    where object_id = object_id(N'[dbo].[decision_definition]')
      and type = 'U'
)
begin
    create table decision_definition (
        decision_id uniqueidentifier not null primary key,
        decision_key nvarchar(255) not null,
        name nvarchar(255) not null,
        category nvarchar(255) null,
        description nvarchar(max) null,
        decision_type nvarchar(64) null,
        source_format nvarchar(32) not null constraint df_decision_definition_source_format default N'DMN',
        status nvarchar(32) not null constraint df_decision_definition_status default N'Draft',
        draft_version_tag nvarchar(255) null,
        published_version_tag nvarchar(255) null,
        tags nvarchar(max) not null constraint df_decision_definition_tags default N'[]',
        created_by nvarchar(255) null,
        created_time datetime2 not null constraint df_decision_definition_created_time default sysutcdatetime(),
        modified_by nvarchar(255) null,
        modified_time datetime2 not null constraint df_decision_definition_modified_time default sysutcdatetime()
    )
end;

if not exists (
    select 1
    from sys.indexes
    where name = N'ux_decision_definition_key'
      and object_id = object_id(N'[dbo].[decision_definition]')
)
begin
    create unique index ux_decision_definition_key
        on decision_definition (decision_key)
end;

if not exists (
    select 1
    from sys.objects
    where object_id = object_id(N'[dbo].[decision_version]')
      and type = 'U'
)
begin
    create table decision_version (
        version_id uniqueidentifier not null primary key,
        decision_id uniqueidentifier not null,
        decision_key nvarchar(255) not null,
        version_number bigint not null,
        version_tag nvarchar(255) not null,
        is_published bit not null constraint df_decision_version_is_published default 0,
        is_draft bit not null constraint df_decision_version_is_draft default 0,
        definition_content nvarchar(max) not null,
        checksum nvarchar(255) null,
        publish_comment nvarchar(max) null,
        published_by nvarchar(255) null,
        published_time datetime2 null,
        created_by nvarchar(255) null,
        created_time datetime2 not null constraint df_decision_version_created_time default sysutcdatetime(),
        constraint fk_decision_version_definition
            foreign key (decision_id) references decision_definition (decision_id)
    )
end;

if not exists (
    select 1
    from sys.indexes
    where name = N'ux_decision_version_key_tag'
      and object_id = object_id(N'[dbo].[decision_version]')
)
begin
    create unique index ux_decision_version_key_tag
        on decision_version (decision_key, version_tag)
end;

if not exists (
    select 1
    from sys.indexes
    where name = N'ux_decision_version_key_number'
      and object_id = object_id(N'[dbo].[decision_version]')
)
begin
    create unique index ux_decision_version_key_number
        on decision_version (decision_key, version_number)
end;

if not exists (
    select 1
    from sys.indexes
    where name = N'ix_decision_version_key_published'
      and object_id = object_id(N'[dbo].[decision_version]')
)
begin
    create index ix_decision_version_key_published
        on decision_version (decision_key, is_published, version_number desc)
end;

if not exists (
    select 1
    from sys.objects
    where object_id = object_id(N'[dbo].[decision_execution_log]')
      and type = 'U'
)
begin
    create table decision_execution_log (
        execution_id uniqueidentifier not null primary key,
        decision_key nvarchar(255) not null,
        requested_version_tag nvarchar(255) null,
        resolved_version_tag nvarchar(255) null,
        binding_mode nvarchar(64) null,
        succeeded bit not null,
        trace_id nvarchar(255) null,
        executor nvarchar(255) null,
        error_message nvarchar(max) null,
        matched_rules nvarchar(max) not null constraint df_decision_execution_log_matched_rules default N'[]',
        input_snapshot nvarchar(max) null,
        output_snapshot nvarchar(max) null,
        trace_entries nvarchar(max) not null constraint df_decision_execution_log_trace_entries default N'[]',
        executed_time datetime2 not null constraint df_decision_execution_log_executed_time default sysutcdatetime(),
        elapsed_ms int null
    )
end;

if not exists (
    select 1
    from sys.indexes
    where name = N'ix_decision_execution_log_key_time'
      and object_id = object_id(N'[dbo].[decision_execution_log]')
)
begin
    create index ix_decision_execution_log_key_time
        on decision_execution_log (decision_key, executed_time desc)
end;

if not exists (
    select 1
    from sys.objects
    where object_id = object_id(N'[dbo].[process_decision_reference]')
      and type = 'U'
)
begin
    create table process_decision_reference (
        reference_id uniqueidentifier not null primary key,
        deployment_id nvarchar(255) not null,
        deployment_name nvarchar(255) not null,
        process_definition_key nvarchar(255) null,
        process_definition_name nvarchar(255) null,
        tenant_id nvarchar(255) null,
        is_draft bit not null constraint df_process_decision_reference_is_draft default 0,
        deploy_time datetime2 not null,
        decision_key nvarchar(255) null,
        task_id nvarchar(255) null,
        task_name nvarchar(255) null,
        binding_mode nvarchar(64) null,
        version_tag nvarchar(255) null,
        indexed_time datetime2 not null constraint df_process_decision_reference_indexed_time default sysutcdatetime()
    )
end;

if not exists (
    select 1
    from sys.indexes
    where name = N'ix_process_decision_reference_key'
      and object_id = object_id(N'[dbo].[process_decision_reference]')
)
begin
    create index ix_process_decision_reference_key
        on process_decision_reference (decision_key, is_draft, deploy_time desc)
end;

if not exists (
    select 1
    from sys.indexes
    where name = N'ix_process_decision_reference_deployment'
      and object_id = object_id(N'[dbo].[process_decision_reference]')
)
begin
    create index ix_process_decision_reference_deployment
        on process_decision_reference (deployment_id)
end;
