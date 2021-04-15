using ConfigCat.Cli.Utils;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ConfigCat.Cli.Commands
{
    class Cat : IExecutableCommand
    {
        private readonly IExecutionContextAccessor accessor;

        public Cat(IExecutionContextAccessor accessor)
        {
            this.accessor = accessor;
        }

        public string Name => "whoisthebestcat";

        public string Description => "Well, who?";

        public IEnumerable<string> Aliases => new[] { "cat" };

        public Task<int> InvokeAsync(CancellationToken token)
        {
            this.accessor.ExecutionContext.Output.Write(@"
                                                              oK        
                                                             koK     
                                                           Xk;oK
                                                         Xkl;:oK
      No                                               Xkl;;;:oK
      N0X                                            Xkl;;;;;:oK
      Ko;xK                                        Xkl;;;;;;;:oK
      Ko;;cxK                                    Xkl;;;;;;;;;:oK
      Kl;;;;cxK                                Xkl;;;;;;;;;;;:oK
      Kl;;;;;;cxK                           Xkdl;;;;;;;;;;;;;:oK
      Kl;;;;;;;;cxK                       Xkl;;;;;;;;;;;;;;;;:oK
      Kl;;;;;;;;;;cxK                   Xkl;;;;;;;;;;;;;;;;;;:oK
      Kl;;;;;;;;;;;;cdxxxxxxxxxxxxxxxxxdl;;;;;;;;;;;;;;;;;;;;:lK
      Kl;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;:lK
      Kl;;;;;;;;;;;;:::::::::::::::::::::::::::::;;;;;;;;;;;;:lK
      Kl;;;;;;;;cdkO000000000KK00000KK000K00000000Oxl:;;;;;;;:lK                www
      Kl;;;;;;cxXNNXOkk0kkOKNWMMMMMMMMMN0dc ``` okXWNOo;;;;;;:lK             N0kddodxOX
      Kl;;;;;cOWWKxok0KXK0kxx0WMMMMMMW0            xNWKo;;;;;:lK           NOl:;;;;;;;cxX
      Kl;;;;;dNWKoxXWMMMMMMNkoOWMMMMMO              dNWOc;;;;:lK          Nk:;;;;;;;;;;;oK
      Kl;;;;;xNNxoKWMMMMMMMMNxdXMMMMWl               KW0c;;;;:lK          Kl;;;;;;;;;;;;:dN
      Kl;;;;;xNNkoKWMMMMMMMMXddXMMMMWo               KW0c;;;;:lK          Ko;;;;;;;;;;;;;c0
      Kl;;;;;dXWKddKWMMMMMWXxo0WMMMMMK              kWWk:;;;;:lK           k:;;;;;;;;;;;;:xN
      Kl;;;;;:xNWXkoxO000OkokKWMMMMMMMXd          c0WW0l;;;;;:lK           Xo;;;;;;;;;;;;;oX
      Kl;;;;;;:oOXNX0Ok0kk0XWMMMMMMMMMMWNOdollodkXNNKxc;;;;;;:lK            k:;;;;;;;;;;;;oX
      Kl;;;;;;;;:loxkkkkkkkkkkkkkkkkkkkkkkkkkkkkkxdoc;;;;;;;;:lK            0c;;;;;;;;;;;;oX
      Kl;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;:lK            0c;;;;;;;;;;;;dN
      Kl;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;:lK            Oc;;;;;;;;;;;:kk
  oOdddddddddddddc:;;;;;;;;;;;dd;;;;;bb;;;;;;;;;;:cdddddddddddddddddOo     Nx:;;;;;;;;;;;ln
      NOxxxxxxxx:;;;;;;;;;;;;;;;dO;Ob;;;;;;;;;;;;;;;:xxxxxxxxxKK           Kl;;;;;;;;;;;:kN
oOdddddddddddddddddddoc:;;;;;;;;;;0;;;;;;;;;;:clddddddddddddddddddddddOo  Nx:;;;;;;;;;;:xN
       Xkxxxxxx:;;;;;;;;;;;;;;;;;;0;;;;;;;;;;;;;;;;;;:xxxxxxxxKK         Nk:;;;;;;;;;;:dX
  oOddddddoooooooooc:;;;;;;;;;;;;;o;;;;;;;;;;;;;;:ccccccccccclddddddOo  Xx:;;;;;;;;;;:xX
        Xd;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;:kK       0o:;;;;;;;;;;lON
         Xd:;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;:kK     Kxc;;;;;;;;;;cxK 
          NOl;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;:kK  N0dc;;;;;;;;;;cd0N
            Xkl;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;:xX0xo:;;;;;;;;;:lxK 
              XOoc;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;cc:;;;;;;;;cox0N 
                 KOxoc:;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;cloxOKN
                      cccdxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxbl

");

            return Task.FromResult(Constants.ExitCodes.Ok);
        }
    }
}
