using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autofac;
using ILogHelper;
using System.Reflection;
using Autofac.Integration.Mvc;
using System.Web.Mvc;

namespace AutofacDemo
{
    public partial class Form1 : Form
    {
        public Form1()
        {

            InitializeComponent();
        }
        public Form1(BaseLog log)
        {
            this.log = log;
            InitializeComponent();
        }
        Autofac.IContainer container;

        BaseLog log;
        private void button1_Click(object sender, EventArgs e)
        {
            
            // user = container.Resolve<IUserService>();
            //log= container.Resolve<BaseLog>();
            //MessageBox.Show(user.Show("abc"));
            MessageBox.Show(log.Debug("debug"));
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ContainerBuilder builder = new ContainerBuilder();
            var ass = Assembly.Load("ILogHelper");
            builder.RegisterAssemblyTypes(ass, ass);
            //builder.RegisterType<UserService>().As<IUserService>();
            ////.AsImplementedInterfaces();
            //builder.RegisterType<NLog>().As<BaseLog>();
            //var assembly = Assembly.GetExecutingAssembly();
            //builder.RegisterAssemblyTypes(assembly).AsImplementedInterfaces();
            builder.RegisterControllers(Assembly.GetExecutingAssembly()).PropertiesAutowired();

            container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
            
            MessageBox.Show("配置完成");
        }
    }
}
