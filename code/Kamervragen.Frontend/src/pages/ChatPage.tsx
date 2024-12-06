import { makeStyles, tokens } from '@fluentui/react-components';
import { Chatlist } from '../components/Chat/Chatlist';
import ChatInterface from '../components/Chat/ChatInterface';
import { useChats } from '../hooks/useChats';
import SearchInterface from '../components/Search/SearchInterface';
import { SelectedQAPair } from '../components/Search/QuestionAnswerList';
import { useState } from 'react';

const useClasses = makeStyles({
  container: {
    // display: 'flex',
    // height: '100vh',
    // width: '100vw',
    display: 'flex',
    flexDirection: 'row',
    alignContent: 'start',
    height: '100vh',
    width: '100vw'
  },
//   leftColumn: {
//     width: '20%',    
//     height: '100vh',
//     // Additional styling if needed
//   },
//   middleColumn: {
//     width:'40%',
//     height: '100vh',
//     // Additional styling if needed
//   },
//   rightColumn: {
//     width:'40%',
//     height: '100vh',
//     // Additional styling if needed
//   },
//   header:
//     {
//         display: 'flex',
//         flexDirection: 'row',
//         justifyContent: 'space-between',
//         alignItems: 'center',
//         padding: '10px',
//         backgroundColor: tokens.colorNeutralBackground4,
//     }
});

export function ChatPage() {
  const classes = useClasses();
  const chats = useChats();
  const [selectedQAPairs, setSelectedQAPairs] = useState<SelectedQAPair[]>([]);

  const handleQAPairsSelected = (qaPairs: SelectedQAPair[]) => {
    setSelectedQAPairs(qaPairs);
  };

  return (
    <div className={classes.container}>
            <Chatlist 
                chats={chats.chats} 
                selectedChatId={chats.selectedChatId} 
                selectChat={chats.selectChat} 
                addChat={chats.addChat} 
                deleteChat={chats.deleteChat} 
                loading={chats.isPending} />
            <ChatInterface 
                selectedChatId={chats.selectedChatId} 
                selectedDocuments={undefined}
                selectedQAPairs={selectedQAPairs} />
        </div>
    // <>
     
    //     <header className={classes.header}>
    //         <h1></h1>
    //     </header>
    //     <div className={classes.container}>
    //         <div className={classes.leftColumn}>
    //             <Chatlist
    //                 chats={chats.chats}
    //                 selectedChatId={chats.selectedChatId}
    //                 selectChat={chats.selectChat}
    //                 addChat={chats.addChat}
    //                 deleteChat={chats.deleteChat}
    //                 loading={chats.isPending} />
    //         </div>
    //         <div className={classes.middleColumn}>
    //             <SearchInterface
    //                 chatId={chats.selectedChatId}
    //                 onQAPairsSelected={handleQAPairsSelected} />
    //         </div>
    //         <div className={classes.rightColumn}>
    //             <ChatInterface
    //                 selectedChatId={chats.selectedChatId}
    //                 selectedQAPairs={selectedQAPairs}
    //                 selectedDocuments={undefined} />
    //         </div>
    //     </div></>
  );
}