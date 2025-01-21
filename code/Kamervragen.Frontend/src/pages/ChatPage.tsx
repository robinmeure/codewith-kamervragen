import { makeStyles } from '@fluentui/react-components';
import { Chatlist } from '../components/Chat/Chatlist';
import ChatInterface from '../components/Chat/ChatInterface';
import { useChats } from '../hooks/useChats';
import { SelectedQAPair } from '../components/Search/QuestionAnswerList';
import { useState } from 'react';

const useClasses = makeStyles({
  container: {
    display: 'flex',
    flexDirection: 'row',
    alignContent: 'start',
    height: '100vh',
    width: '100vw'
  },
});

export function ChatPage() {
  const classes = useClasses();
  const chats = useChats();
  const [selectedQAPairs] = useState<SelectedQAPair[]>([]);

  

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
   
  );
}
