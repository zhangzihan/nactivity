using System;
using System.Collections.Generic;

/* Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * 
 *      http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace org.activiti.engine.impl.bpmn.behavior
{
    using org.activiti.engine.@delegate;
    using org.activiti.engine.impl.persistence.entity;
    using Sys.Data;

    /// 
    /// 
    /// 
    [Serializable]
    public class MailActivityBehavior : AbstractBpmnActivityBehavior
    {
        private const long serialVersionUID = 1L;

        private static readonly Type[] ALLOWED_ATT_TYPES = new Type[] {
          typeof(File),
          typeof(File[]), typeof(string), typeof(string[]), typeof(IDataSource), typeof(IDataSource[])};

        protected internal IExpression to;
        protected internal IExpression from;
        protected internal IExpression cc;
        protected internal IExpression bcc;
        protected internal IExpression subject;
        protected internal IExpression text;
        protected internal IExpression textVar;
        protected internal IExpression html;
        protected internal IExpression htmlVar;
        protected internal IExpression charset;
        protected internal IExpression ignoreException;
        protected internal IExpression exceptionVariableName;
        protected internal IExpression attachments;

        public override void Execute(IExecutionEntity execution)
        {
            throw new NotImplementedException();
            //bool doIgnoreException = bool.Parse(getStringFromField(ignoreException, execution));
            //string exceptionVariable = getStringFromField(exceptionVariableName, execution);
            //Email email = null;
            //try
            //{
            //    string toStr = getStringFromField(to, execution);
            //    string fromStr = getStringFromField(from, execution);
            //    string ccStr = getStringFromField(cc, execution);
            //    string bccStr = getStringFromField(bcc, execution);
            //    string subjectStr = getStringFromField(subject, execution);
            //    string textStr = textVar == null ? getStringFromField(text, execution) : getStringFromField(getExpression(execution, textVar), execution);
            //    string htmlStr = htmlVar == null ? getStringFromField(html, execution) : getStringFromField(getExpression(execution, htmlVar), execution);
            //    string charSetStr = getStringFromField(charset, execution);
            //    IList<File> files = new LinkedList<File>();
            //    IList<DataSource> dataSources = new LinkedList<DataSource>();
            //    getFilesFromFields(attachments, execution, files, dataSources);

            //    email = createEmail(textStr, htmlStr, attachmentsExist(files, dataSources));
            //    addTo(email, toStr);
            //    setFrom(email, fromStr, execution.TenantId);
            //    addCc(email, ccStr);
            //    addBcc(email, bccStr);
            //    setSubject(email, subjectStr);
            //    setMailServerProperties(email, execution.TenantId);
            //    setCharset(email, charSetStr);
            //    attach(email, files, dataSources);

            //    email.send();

            //}
            //catch (ActivitiException e)
            //{
            //    handleException(execution, e.Message, e, doIgnoreException, exceptionVariable);
            //}
            //catch (EmailException e)
            //{
            //    handleException(execution, "Could not send e-mail in execution " + execution.Id, e, doIgnoreException, exceptionVariable);
            //}

            //leave(execution);
        }

        private bool AttachmentsExist(IList<File> files, IList<IDataSource> dataSources)
        {
            throw new NotImplementedException();
            //return !((files == null || files.Count == 0) && (dataSources == null || dataSources.Count == 0));
        }

        protected internal virtual Email CreateEmail(string text, string html, bool attachmentsExist)
        {
            throw new NotImplementedException();
            //if (!string.ReferenceEquals(html, null))
            //{
            //    return createHtmlEmail(text, html);
            //}
            //else if (!string.ReferenceEquals(text, null))
            //{
            //    if (!attachmentsExist)
            //    {
            //        return createTextOnlyEmail(text);
            //    }
            //    else
            //    {
            //        return createMultiPartEmail(text);
            //    }
            //}
            //else
            //{
            //    throw new ActivitiIllegalArgumentException("'html' or 'text' is required to be defined when using the mail activity");
            //}
        }

        protected internal virtual HtmlEmail CreateHtmlEmail(string text, string html)
        {
            throw new NotImplementedException();
            //HtmlEmail email = new HtmlEmail();
            //try
            //{
            //    email.HtmlMsg = html;
            //    if (!string.ReferenceEquals(text, null))
            //    { // for email clients that don't support html
            //        email.TextMsg = text;
            //    }
            //    return email;
            //}
            //catch (EmailException e)
            //{
            //    throw new ActivitiException("Could not create HTML email", e);
            //}
        }

        protected internal virtual SimpleEmail CreateTextOnlyEmail(string text)
        {
            throw new NotImplementedException();
            //SimpleEmail email = new SimpleEmail();
            //try
            //{
            //    email.Msg = text;
            //    return email;
            //}
            //catch (EmailException e)
            //{
            //    throw new ActivitiException("Could not create text-only email", e);
            //}
        }

        protected internal virtual MultiPartEmail CreateMultiPartEmail(string text)
        {
            throw new NotImplementedException();
            //MultiPartEmail email = new MultiPartEmail();
            //try
            //{
            //    email.Msg = text;
            //    return email;
            //}
            //catch (EmailException e)
            //{
            //    throw new ActivitiException("Could not create text-only email", e);
            //}
        }

        protected internal virtual void AddTo(Email email, string to)
        {
            throw new NotImplementedException();
            //string[] tos = splitAndTrim(to);
            //    if (tos != null)
            //    {
            //        foreach (string t in tos)
            //        {
            //            try
            //            {
            //                email.addTo(t);
            //            }
            //            catch (EmailException e)
            //            {
            //                throw new ActivitiException("Could not add " + t + " as recipient", e);
            //            }
            //        }
            //    }
            //    else
            //    {
            //        throw new ActivitiException("No recipient could be found for sending email");
            //    }
        }

        protected internal virtual void SetFrom(Email email, string from, string tenantId)
        {
            throw new NotImplementedException();
            //string fromAddress = null;

            //if (!string.ReferenceEquals(from, null))
            //{
            //    fromAddress = from;
            //}
            //else
            //{ // use default configured from address in process engine config
            //    if (!string.ReferenceEquals(tenantId, null) && tenantId.Length > 0)
            //    {
            //        IDictionary<string, MailServerInfo> mailServers = Context.ProcessEngineConfiguration.MailServers;
            //        if (mailServers != null && mailServers.ContainsKey(tenantId))
            //        {
            //            MailServerInfo mailServerInfo = mailServers[tenantId];
            //            fromAddress = mailServerInfo.MailServerDefaultFrom;
            //        }
            //    }

            //    if (string.ReferenceEquals(fromAddress, null))
            //    {
            //        fromAddress = Context.ProcessEngineConfiguration.MailServerDefaultFrom;
            //    }
            //}

            //try
            //{
            //    email.From = fromAddress;
            //}
            //catch (EmailException e)
            //{
            //    throw new ActivitiException("Could not set " + from + " as from address in email", e);
            //}
        }

        protected internal virtual void AddCc(Email email, string cc)
        {
            throw new NotImplementedException();
            //string[] ccs = splitAndTrim(cc);
            //if (ccs != null)
            //{
            //    foreach (string c in ccs)
            //    {
            //        try
            //        {
            //            email.addCc(c);
            //        }
            //        catch (EmailException e)
            //        {
            //            throw new ActivitiException("Could not add " + c + " as cc recipient", e);
            //        }
            //    }
            //}
        }

        protected internal virtual void AddBcc(Email email, string bcc)
        {
            throw new NotImplementedException();
            //string[] bccs = splitAndTrim(bcc);
            //if (bccs != null)
            //{
            //    foreach (string b in bccs)
            //    {
            //        try
            //        {
            //            email.addBcc(b);
            //        }
            //        catch (EmailException e)
            //        {
            //            throw new ActivitiException("Could not add " + b + " as bcc recipient", e);
            //        }
            //    }
            //}
        }

        protected internal virtual void Attach(Email email, IList<File> files, IList<IDataSource> dataSources)
        {
            throw new NotImplementedException();
            //if (!(email is MultiPartEmail && attachmentsExist(files, dataSources)))
            //{
            //    return;
            //}
            //MultiPartEmail mpEmail = (MultiPartEmail)email;
            //foreach (File file in files)
            //{
            //    mpEmail.attach(file);
            //}
            //foreach (DataSource ds in dataSources)
            //{
            //    if (ds != null)
            //    {
            //        mpEmail.attach(ds, ds.Name, null);
            //    }
            //}
        }

        protected internal virtual void SetSubject(Email email, string subject)
        {
            throw new NotImplementedException();
            //email.Subject = !string.ReferenceEquals(subject, null) ? subject : "";
        }

        protected internal virtual void SetMailServerProperties(Email email, string tenantId)
        {
            throw new NotImplementedException();
            //ProcessEngineConfigurationImpl processEngineConfiguration = Context.ProcessEngineConfiguration;

            //bool isMailServerSet = false;
            //if (!string.ReferenceEquals(tenantId, null) && tenantId.Length > 0)
            //{
            //    if (!string.ReferenceEquals(processEngineConfiguration.getMailSessionJndi(tenantId), null))
            //    {
            //        setEmailSession(email, processEngineConfiguration.getMailSessionJndi(tenantId));
            //        isMailServerSet = true;

            //    }
            //    else if (processEngineConfiguration.getMailServer(tenantId) != null)
            //    {
            //        MailServerInfo mailServerInfo = processEngineConfiguration.getMailServer(tenantId);
            //        string host = mailServerInfo.MailServerHost;
            //        if (string.ReferenceEquals(host, null))
            //        {
            //            throw new ActivitiException("Could not send email: no SMTP host is configured for tenantId " + tenantId);
            //        }
            //        email.HostName = host;

            //        email.SmtpPort = mailServerInfo.MailServerPort;

            //        email.SSLOnConnect = mailServerInfo.MailServerUseSSL;
            //        email.StartTLSEnabled = mailServerInfo.MailServerUseTLS;

            //        string user = mailServerInfo.MailServerUsername;
            //        string password = mailServerInfo.MailServerPassword;
            //        if (!string.ReferenceEquals(user, null) && !string.ReferenceEquals(password, null))
            //        {
            //            email.setAuthentication(user, password);
            //        }

            //        isMailServerSet = true;
            //    }
            //}

            //if (!isMailServerSet)
            //{
            //    string mailSessionJndi = processEngineConfiguration.MailSessionJndi;
            //    if (!string.ReferenceEquals(mailSessionJndi, null))
            //    {
            //        setEmailSession(email, mailSessionJndi);

            //    }
            //    else
            //    {
            //        string host = processEngineConfiguration.MailServerHost;
            //        if (string.ReferenceEquals(host, null))
            //        {
            //            throw new ActivitiException("Could not send email: no SMTP host is configured");
            //        }
            //        email.HostName = host;

            //        int port = processEngineConfiguration.MailServerPort;
            //        email.SmtpPort = port;

            //        email.SSLOnConnect = processEngineConfiguration.MailServerUseSSL;
            //        email.StartTLSEnabled = processEngineConfiguration.MailServerUseTLS;

            //        string user = processEngineConfiguration.MailServerUsername;
            //        string password = processEngineConfiguration.MailServerPassword;
            //        if (!string.ReferenceEquals(user, null) && !string.ReferenceEquals(password, null))
            //        {
            //            email.setAuthentication(user, password);
            //        }
            //    }
            //}
        }

        protected internal virtual void SetEmailSession(Email email, string mailSessionJndi)
        {
            throw new NotImplementedException();
            //try
            //{
            //    email.MailSessionFromJNDI = mailSessionJndi;
            //}
            //catch (NamingException e)
            //{
            //    throw new ActivitiException("Could not send email: Incorrect JNDI configuration", e);
            //}
        }

        protected internal virtual void SetCharset(Email email, string charSetStr)
        {
            throw new NotImplementedException();
            //if (charset != null)
            //{
            //    email.Charset = charSetStr;
            //}
        }

        protected internal virtual string[] SplitAndTrim(string str)
        {
            throw new NotImplementedException();
            //if (!string.ReferenceEquals(str, null))
            //{
            //    string[] splittedStrings = str.Split(",", true);
            //    for (int i = 0; i < splittedStrings.Length; i++)
            //    {
            //        splittedStrings[i] = splittedStrings[i].Trim();
            //    }
            //    return splittedStrings;
            //}
            //return null;
        }

        protected internal virtual string GetStringFromField(IExpression expression, IExecutionEntity execution)
        {
            throw new NotImplementedException();
            //if (expression != null)
            //{
            //    object value = expression.getValue(execution);
            //    if (value != null)
            //    {
            //        return value.ToString();
            //    }
            //}
            //return null;
        }

        private void GetFilesFromFields(IExpression expression, IExecutionEntity execution, IList<File> files, IList<IDataSource> dataSources)
        {
            throw new NotImplementedException();
            //object value = checkAllowedTypes(expression, execution);
            //if (value != null)
            //{
            //    if (value is File)
            //    {
            //        files.Add((File)value);
            //    }
            //    else if (value is string)
            //    {
            //        files.Add(new File((string)value));
            //    }
            //    else if (value is File[])
            //    {
            //        Collections.addAll(files, (File[])value);
            //    }
            //    else if (value is string[])
            //    {
            //        string[] paths = (string[])value;
            //        foreach (string path in paths)
            //        {
            //            files.Add(new File(path));
            //        }
            //    }
            //    else if (value is DataSource)
            //    {
            //        dataSources.Add((DataSource)value);
            //    }
            //    else if (value is DataSource[])
            //    {
            //        foreach (DataSource ds in (DataSource[])value)
            //        {
            //            if (ds != null)
            //            {
            //                dataSources.Add(ds);
            //            }
            //        }
            //    }
            //}
            //for (IEnumerator<File> it = files.GetEnumerator(); it.MoveNext();)
            //{
            //    File file = it.Current;
            //    if (!fileExists(file))
            //    {
            //        it.remove();
            //    }
            //}
        }

        private object CheckAllowedTypes(IExpression expression, IExecutionEntity execution)
        {
            throw new NotImplementedException();
            //if (expression == null)
            //{
            //    return null;
            //}
            //object value = expression.getValue(execution);
            //if (value == null)
            //{
            //    return null;
            //}
            //foreach (Type allowedType in ALLOWED_ATT_TYPES)
            //{
            //    if (allowedType.IsInstanceOfType(value))
            //    {
            //        return value;
            //    }
            //}
            //throw new ActivitiException("Invalid attachment type: " + value.GetType());
        }

        protected internal virtual bool FileExists(File file)
        {
            throw new NotImplementedException();
            //return file != null && file.exists() && file.File && file.canRead();
        }

        protected internal virtual IExpression GetExpression(IExecutionEntity execution, IExpression var)
        {
            throw new NotImplementedException();
            //string variable = (string)execution.getVariable(var.ExpressionText);
            //return Context.ProcessEngineConfiguration.ExpressionManager.createExpression(variable);
        }

        protected internal virtual void HandleException(IExecutionEntity execution, string msg, Exception e, bool doIgnoreException, string exceptionVariable)
        {
            throw new NotImplementedException();
            //if (doIgnoreException)
            //{
            //    LOG.info("Ignoring email send error: " + msg, e);
            //    if (!string.ReferenceEquals(exceptionVariable, null) && exceptionVariable.Length > 0)
            //    {
            //        execution.setVariable(exceptionVariable, msg);
            //    }
            //}
            //else
            //{
            //    if (e is ActivitiException)
            //    {
            //        throw (ActivitiException)e;
            //    }
            //    else
            //    {
            //        throw new ActivitiException(msg, e);
            //    }
            //}
        }

        public class File
        {
        }

        public class Email
        {
        }

        public class HtmlEmail
        {
        }

        public class SimpleEmail
        {
        }

        public class MultiPartEmail
        {
        }
    }

}