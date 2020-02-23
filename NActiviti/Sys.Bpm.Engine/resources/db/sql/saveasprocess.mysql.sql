set @deploy_id = uuid();

/*流程部署*/
insert into activiti.ACT_RE_DEPLOYMENT(ID_, NAME_, BUSINESS_KEY_, CATEGORY_, KEY_, TENANT_ID_, DEPLOY_TIME_, ENGINE_VERSION_)
select @deploy_id, b.NAME_, b.BUSINESS_KEY_, b.CATEGORY_, b.KEY_, '替换为其它组织ID', b.DEPLOY_TIME_, b.ENGINE_VERSION_
 from activiti.ACT_RE_PROCDEF a inner join activiti.ACT_RE_DEPLOYMENT b on a.DEPLOYMENT_ID_ = b.id_ 
where a.name_ = '替换为带复制流程名称'
order by a.version_ desc limit 0, 1;

/*流程定义*/
insert into activiti.ACT_RE_PROCDEF(ID_, REV_, CATEGORY_, NAME_, BUSINESS_KEY_, BUSINESS_PATH_, START_FORM_, KEY_, VERSION_, DEPLOYMENT_ID_, RESOURCE_NAME_,
DGRM_RESOURCE_NAME_, DESCRIPTION_, HAS_START_FORM_KEY_, HAS_GRAPHICAL_NOTATION_, SUSPENSION_STATE_, TENANT_ID_, ENGINE_VERSION_)
select uuid(), a.REV_, a.CATEGORY_, a.NAME_, a.BUSINESS_KEY_, a.BUSINESS_PATH_, a.START_FORM_, a.KEY_, 1, @deploy_id, a.RESOURCE_NAME_,
a.DGRM_RESOURCE_NAME_, a.DESCRIPTION_, a.HAS_START_FORM_KEY_, a.HAS_GRAPHICAL_NOTATION_, a.SUSPENSION_STATE_, '替换为其它组织ID', a.ENGINE_VERSION_
from activiti.ACT_RE_PROCDEF a inner join activiti.ACT_RE_DEPLOYMENT b on a.DEPLOYMENT_ID_ = b.id_ 
where a.name_ = '替换为带复制流程名称'
order by a.version_ desc limit 0, 1;

/*流程定义二进制数据*/
insert into activiti.ACT_GE_BYTEARRAY(ID_, REV_, NAME_, DEPLOYMENT_ID_, BYTES_, GENERATED_)
select uuid(), c.REV_, c.NAME_, @deploy_id, c.BYTES_, c.GENERATED_ 
from activiti.ACT_RE_PROCDEF a 
	inner join activiti.ACT_RE_DEPLOYMENT b on a.DEPLOYMENT_ID_ = b.id_ 
	inner join activiti.ACT_GE_BYTEARRAY c on b.id_ = c.deployment_id_
where a.name_ = '替换为带复制流程名称'
order by a.version_ desc limit 0, 1;