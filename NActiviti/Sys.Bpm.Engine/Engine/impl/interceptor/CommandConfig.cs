namespace org.activiti.engine.impl.interceptor
{
	using org.activiti.engine.impl.cfg;

	/// <summary>
	/// Configuration settings for the command interceptor chain.
	/// 
	/// Instances of this class are immutable, and thus thread- and share-safe.
	/// 
	/// 
	/// </summary>
	public class CommandConfig
	{

	  private bool contextReusePossible;
	  private TransactionPropagation propagation;

	  public CommandConfig()
	  {
		this.contextReusePossible = true;
		this.propagation = TransactionPropagation.REQUIRED;
	  }

	  public CommandConfig(bool contextReusePossible)
	  {
		this.contextReusePossible = contextReusePossible;
		this.propagation = TransactionPropagation.REQUIRED;
	  }

	  public CommandConfig(bool contextReusePossible, TransactionPropagation transactionPropagation)
	  {
		this.contextReusePossible = contextReusePossible;
		this.propagation = transactionPropagation;
	  }

	  protected internal CommandConfig(CommandConfig commandConfig)
	  {
		this.contextReusePossible = commandConfig.contextReusePossible;
		this.propagation = commandConfig.propagation;
	  }

	  public virtual bool ContextReusePossible
	  {
		  get
		  {
			return contextReusePossible;
		  }
	  }

	  public virtual TransactionPropagation TransactionPropagation
	  {
		  get
		  {
			return propagation;
		  }
	  }

	  public virtual CommandConfig setContextReusePossible(bool contextReusePossible)
	  {
		CommandConfig config = new CommandConfig(this);
		config.contextReusePossible = contextReusePossible;
		return config;
	  }

	  public virtual CommandConfig transactionRequired()
	  {
		CommandConfig config = new CommandConfig(this);
		config.propagation = TransactionPropagation.REQUIRED;
		return config;
	  }

	  public virtual CommandConfig transactionRequiresNew()
	  {
		CommandConfig config = new CommandConfig();
		config.contextReusePossible = false;
		config.propagation = TransactionPropagation.REQUIRES_NEW;
		return config;
	  }

	  public virtual CommandConfig transactionNotSupported()
	  {
		CommandConfig config = new CommandConfig();
		config.contextReusePossible = false;
		config.propagation = TransactionPropagation.NOT_SUPPORTED;
		return config;
	  }
	}

}