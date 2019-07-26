/*清除历史数据*/
delete from activiti.ACT_HI_PROCINST;
delete from activiti.ACT_HI_ACTINST;
delete from activiti.ACT_HI_VARINST;
delete from activiti.ACT_HI_TASKINST;
delete from activiti.ACT_HI_DETAIL;
delete from activiti.ACT_HI_COMMENT;
delete from activiti.ACT_HI_ATTACHMENT;
delete from activiti.ACT_HI_IDENTITYLINK;

/*清除运行时数据*/
delete from activiti.ACT_RU_VARIABLE;
delete from activiti.ACT_RE_MODEL;
delete from activiti.ACT_RU_IDENTITYLINK;
delete from activiti.ACT_RU_JOB;
delete from activiti.ACT_RU_TIMER_JOB;
delete from activiti.ACT_RU_SUSPENDED_JOB;
delete from activiti.ACT_RU_DEADLETTER_JOB;
delete from activiti.ACT_RU_EVENT_SUBSCR;
delete from activiti.ACT_RU_TASK;
delete from activiti.ACT_RU_EXECUTION;
delete from activiti.ACT_EVT_LOG;
delete from activiti.ACT_PROCDEF_INFO;
delete from activiti.ACT_RU_INTEGRATION;
delete from activiti.ACT_GE_BYTEARRAY where DEPLOYMENT_ID_ IS NULL;
